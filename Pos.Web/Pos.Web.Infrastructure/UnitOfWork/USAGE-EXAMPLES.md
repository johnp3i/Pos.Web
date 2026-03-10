# Unit of Work Pattern - Usage Examples

This document provides comprehensive examples of how to use the Unit of Work pattern in the POS system.

## Overview

The Unit of Work pattern coordinates multiple repository operations in a single transaction, ensuring data consistency and providing a clean abstraction for database operations.

## Key Features

- **Transaction Management**: Begin, Commit, and Rollback transactions
- **Lazy Repository Initialization**: Repositories are created only when accessed
- **Shared DbContext**: All repositories use the same DbContext instance
- **Automatic Cleanup**: Proper disposal of resources
- **JDS Compliance**: Follows JDS repository design guidelines

---

## Basic Usage

### Example 1: Simple Order Creation

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<int> CreateOrderAsync(Order order)
    {
        // Add order using repository
        var createdOrder = await _unitOfWork.Orders.AddAsync(order);
        
        // SaveChangesAsync is called automatically by AddAsync
        // No need to call _unitOfWork.SaveChangesAsync() here
        
        return createdOrder.ID;
    }
}
```

### Example 2: Multiple Operations Without Transaction

```csharp
public async Task<Customer> CreateCustomerWithAddressAsync(
    Customer customer, 
    CustomerAddress address)
{
    // Add customer
    var createdCustomer = await _unitOfWork.Customers.AddAsync(customer);
    
    // Set customer ID on address
    address.CustomerID = createdCustomer.ID;
    
    // Add address
    await _unitOfWork.CustomerAddresses.AddAsync(address);
    
    // Both operations are saved independently
    // If second operation fails, first is already committed
    
    return createdCustomer;
}
```

---

## Transaction Management

### Example 3: Order Creation with Transaction

```csharp
public async Task<int> CreateOrderWithTransactionAsync(
    Order order, 
    List<OrderItem> items)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Add order (without SaveChanges)
        order.TimeStamp = DateTime.UtcNow;
        _unitOfWork.Orders.AddAsync(order);
        
        // Add order items
        foreach (var item in items)
        {
            item.InvoiceID = order.ID;
            await _unitOfWork.OrderItems.AddAsync(item);
        }
        
        // Save all changes
        await _unitOfWork.SaveChangesAsync();
        
        // Commit transaction
        await _unitOfWork.CommitAsync();
        
        return order.ID;
    }
    catch (Exception)
    {
        // Rollback on error
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

### Example 4: Complex Multi-Repository Transaction

```csharp
public async Task<int> ProcessOrderWithLoyaltyPointsAsync(
    Order order, 
    int customerId, 
    int loyaltyPointsToAdd)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // 1. Create order
        var createdOrder = await _unitOfWork.Orders.AddAsync(order);
        
        // 2. Update customer loyalty points
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer != null)
        {
            customer.LoyaltyPoints += loyaltyPointsToAdd;
            await _unitOfWork.Customers.UpdateAsync(customer);
        }
        
        // 3. Log audit entry
        var auditLog = new ApiAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserID = order.UserID,
            Action = "OrderCreated",
            EntityType = "Order",
            EntityID = createdOrder.ID,
            Changes = $"Order created with {loyaltyPointsToAdd} loyalty points"
        };
        await _unitOfWork.ApiAuditLogs.AddAsync(auditLog);
        
        // 4. Save all changes
        await _unitOfWork.SaveChangesAsync();
        
        // 5. Commit transaction
        await _unitOfWork.CommitAsync();
        
        return createdOrder.ID;
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

---

## Order Locking with Unit of Work

### Example 5: Acquire Order Lock

```csharp
public async Task<bool> AcquireOrderLockAsync(
    int orderId, 
    int userId, 
    string sessionId)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Check if order is already locked
        var existingLocks = await _unitOfWork.OrderLocks.FindAsync(
            l => l.OrderID == orderId && l.IsActive);
        
        if (existingLocks.Any())
        {
            await _unitOfWork.RollbackAsync();
            return false; // Order is already locked
        }
        
        // Create new lock
        var orderLock = new OrderLock
        {
            OrderID = orderId,
            UserID = userId,
            SessionID = sessionId,
            LockAcquiredAt = DateTime.UtcNow,
            LockExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.OrderLocks.AddAsync(orderLock);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();
        
        return true;
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

### Example 6: Release Order Lock

```csharp
public async Task ReleaseOrderLockAsync(int orderId, int userId)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Find active lock for this order and user
        var locks = await _unitOfWork.OrderLocks.FindAsync(
            l => l.OrderID == orderId && 
                 l.UserID == userId && 
                 l.IsActive);
        
        foreach (var orderLock in locks)
        {
            orderLock.IsActive = false;
            orderLock.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.OrderLocks.UpdateAsync(orderLock);
        }
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

---

## Offline Sync with Unit of Work

### Example 7: Process Sync Queue

```csharp
public async Task ProcessSyncQueueAsync(int userId, string deviceId)
{
    // Get pending sync items
    var pendingItems = await _unitOfWork.SyncQueues.FindAsync(
        s => s.UserID == userId && 
             s.DeviceID == deviceId && 
             s.Status == "Pending");
    
    foreach (var syncItem in pendingItems)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Process sync item based on operation type
            switch (syncItem.OperationType)
            {
                case "CreateOrder":
                    await ProcessCreateOrderSync(syncItem);
                    break;
                case "UpdateCustomer":
                    await ProcessUpdateCustomerSync(syncItem);
                    break;
                // ... other operations
            }
            
            // Mark sync item as completed
            syncItem.Status = "Completed";
            syncItem.ServerTimestamp = DateTime.UtcNow;
            await _unitOfWork.SyncQueues.UpdateAsync(syncItem);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            
            // Mark sync item as failed
            syncItem.Status = "Failed";
            syncItem.ErrorMessage = ex.Message;
            syncItem.AttemptCount++;
            await _unitOfWork.SyncQueues.UpdateAsync(syncItem);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

---

## Feature Flag Management

### Example 8: Check and Update Feature Flag

```csharp
public async Task<bool> IsFeatureEnabledAsync(string featureName, int userId)
{
    var featureFlag = (await _unitOfWork.FeatureFlags.FindAsync(
        f => f.Name == featureName)).FirstOrDefault();
    
    if (featureFlag == null)
        return false;
    
    return featureFlag.IsEnabled;
}

public async Task UpdateFeatureFlagAsync(
    string featureName, 
    bool isEnabled, 
    int updatedBy)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        var featureFlag = (await _unitOfWork.FeatureFlags.FindAsync(
            f => f.Name == featureName)).FirstOrDefault();
        
        if (featureFlag != null)
        {
            featureFlag.IsEnabled = isEnabled;
            featureFlag.UpdatedAt = DateTime.UtcNow;
            featureFlag.UpdatedBy = updatedBy;
            
            await _unitOfWork.FeatureFlags.UpdateAsync(featureFlag);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
```

---

## Dependency Injection Setup

### Example 9: Register Unit of Work in Program.cs

```csharp
// In Program.cs or Startup.cs

// Register DbContext
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Unit of Work with scoped lifetime
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repositories (optional, if used directly)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

### Example 10: Use Unit of Work in Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(
        IUnitOfWork unitOfWork, 
        ILogger<OrdersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Create order
            var order = new Order
            {
                UserID = request.UserId,
                CustomerID = request.CustomerId,
                TotalCost = request.TotalCost,
                TimeStamp = DateTime.UtcNow,
                Status = "Pending"
            };
            
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            
            // Add order items
            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    InvoiceID = createdOrder.ID,
                    CategoryItemID = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                };
                
                await _unitOfWork.OrderItems.AddAsync(orderItem);
            }
            
            // Log audit entry
            var auditLog = new ApiAuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserID = request.UserId,
                Action = "CreateOrder",
                EntityType = "Order",
                EntityID = createdOrder.ID,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                RequestPath = HttpContext.Request.Path
            };
            
            await _unitOfWork.ApiAuditLogs.AddAsync(auditLog);
            
            // Save and commit
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Order {OrderId} created successfully", createdOrder.ID);
            
            return Ok(new { orderId = createdOrder.ID });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Failed to create order");
            return StatusCode(500, "Failed to create order");
        }
    }
}
```

---

## Best Practices

### 1. Always Use Transactions for Multi-Step Operations

```csharp
// ✅ GOOD: Use transaction for multiple operations
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Orders.AddAsync(order);
    await _unitOfWork.Customers.UpdateAsync(customer);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitAsync();
}
catch
{
    await _unitOfWork.RollbackAsync();
    throw;
}

// ❌ BAD: No transaction, partial data on failure
await _unitOfWork.Orders.AddAsync(order);
await _unitOfWork.Customers.UpdateAsync(customer);
```

### 2. Always Rollback on Exception

```csharp
// ✅ GOOD: Rollback in catch block
try
{
    await _unitOfWork.BeginTransactionAsync();
    // ... operations
    await _unitOfWork.CommitAsync();
}
catch
{
    await _unitOfWork.RollbackAsync();
    throw;
}

// ❌ BAD: No rollback, transaction left open
try
{
    await _unitOfWork.BeginTransactionAsync();
    // ... operations
    await _unitOfWork.CommitAsync();
}
catch
{
    throw; // Transaction still open!
}
```

### 3. Use Scoped Lifetime in DI

```csharp
// ✅ GOOD: Scoped lifetime (one per request)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ❌ BAD: Singleton lifetime (shared across requests)
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
```

### 4. Don't Nest Transactions

```csharp
// ❌ BAD: Nested transactions
await _unitOfWork.BeginTransactionAsync();
await _unitOfWork.BeginTransactionAsync(); // Throws exception!

// ✅ GOOD: Single transaction
await _unitOfWork.BeginTransactionAsync();
// ... all operations
await _unitOfWork.CommitAsync();
```

### 5. Dispose Properly

```csharp
// ✅ GOOD: DI container handles disposal automatically
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork; // DI container disposes
    }
}

// ⚠️ MANUAL: Only if creating manually (not recommended)
using (var unitOfWork = new UnitOfWork(context))
{
    // ... operations
} // Disposed automatically
```

---

## Common Patterns

### Pattern 1: Service Layer with Unit of Work

```csharp
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<OrderResult> CreateOrderAsync(CreateOrderCommand command)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Business logic here
            var order = MapToOrder(command);
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            return OrderResult.Success(createdOrder.ID);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Failed to create order");
            return OrderResult.Failure(ex.Message);
        }
    }
}
```

### Pattern 2: Repository Pattern with Unit of Work

```csharp
// Service uses Unit of Work, not repositories directly
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Customer> GetCustomerWithOrdersAsync(int customerId)
    {
        // Access repository through Unit of Work
        var customer = await _unitOfWork.Customers
            .GetCustomerWithAddressesAsync(customerId);
        
        if (customer != null)
        {
            var orders = await _unitOfWork.Orders
                .GetOrdersByCustomerIdAsync(customerId);
            
            // Process customer and orders
        }
        
        return customer;
    }
}
```

---

## Summary

The Unit of Work pattern provides:

- ✅ **Transaction Management**: Ensure data consistency across multiple operations
- ✅ **Lazy Initialization**: Repositories created only when needed
- ✅ **Shared Context**: All repositories use the same DbContext
- ✅ **Clean Abstraction**: Service layer doesn't need to manage DbContext
- ✅ **JDS Compliance**: Follows all JDS repository design guidelines

For more information, see:
- [JDS Repository Design Guidelines](../../repository-standards.md)
- [Entity Framework Core Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
