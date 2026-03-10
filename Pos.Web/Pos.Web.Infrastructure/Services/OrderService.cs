using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.UnitOfWork;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Models;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Order service implementation for managing order operations
/// Implements order CRUD, validation, stock checking, and order locking integration
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IOrderLockService _orderLockService;
    private readonly IApiAuditLogService _auditLogService;
    private readonly ILogger<OrderService> _logger;
    
    // Tax rate configuration (should come from configuration/database)
    private const decimal TaxRate = 0.10m; // 10% tax rate

    public OrderService(
        IUnitOfWork unitOfWork,
        IProductService productService,
        IOrderLockService orderLockService,
        IApiAuditLogService auditLogService,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
        _orderLockService = orderLockService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int userId)
    {
        try
        {
            _logger.LogInformation("Creating order for user {UserId}", userId);
            
            // Validate order items
            var validationResult = await ValidateOrderItemsAsync(request.Items);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                _logger.LogWarning("Order validation failed: {Errors}", errors);
                throw new OrderValidationException($"Order validation failed: {errors}");
            }
            
            // Calculate order totals
            var calculation = await CalculateOrderTotalsAsync(
                request.Items, 
                request.DiscountPercentage, 
                request.DiscountAmount);
            
            // Create order entity
            var order = new Order
            {
                CustomerID = request.CustomerId,
                UserID = userId,
                ServiceTypeID = (byte)request.ServiceType,
                ServiceType = request.ServiceType.ToString(),
                TableNumber = request.TableNumber,
                Status = OrderStatus.Pending.ToString(),
                Subtotal = calculation.Subtotal,
                TaxAmount = calculation.TaxAmount,
                TotalCost = calculation.TotalAmount,
                TotalAmount = calculation.TotalAmount,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = calculation.DiscountAmount,
                VoucherID = request.VoucherId,
                InvoiceNote = request.Notes,
                Notes = request.Notes,
                IsInvoiceNotePrintable = request.IsNotesPrintable,
                ScheduledTime = request.ScheduledTime,
                TimeStamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            // Add order items
            foreach (var itemDto in request.Items)
            {
                var orderItem = new OrderItem
                {
                    CategoryItemID = itemDto.ProductId, // Legacy: CategoryItemID is ProductID
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    Notes = itemDto.Notes
                };
                order.Items.Add(orderItem);
            }
            
            // Save order
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order created successfully: {OrderId}", order.ID);
            
            // Audit log
            await _auditLogService.LogApiRequestAsync(
                userId: userId,
                action: "CreateOrder",
                requestPath: "/api/orders",
                requestMethod: "POST",
                statusCode: 201,
                duration: 0);
            
            // Return DTO
            return await GetOrderByIdAsync(order.ID) 
                ?? throw new InvalidOperationException("Failed to retrieve created order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<OrderDto> UpdateOrderAsync(UpdateOrderRequest request, int userId)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} by user {UserId}", request.OrderId, userId);
            
            // Check if order is locked by another user
            var lockStatus = await _orderLockService.GetLockStatusAsync(request.OrderId);
            if (lockStatus != null && lockStatus.LockedByUserId != userId)
            {
                _logger.LogWarning("Order {OrderId} is locked by user {LockedByUserId}", 
                    request.OrderId, lockStatus.LockedByUserId);
                throw new OrderLockedException(
                    $"Order is currently being edited by {lockStatus.LockedByUserName}");
            }
            
            // Get existing order
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", request.OrderId);
                throw new OrderNotFoundException($"Order {request.OrderId} not found");
            }
            
            // Validate order items
            var validationResult = await ValidateOrderItemsAsync(request.Items);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                _logger.LogWarning("Order validation failed: {Errors}", errors);
                throw new OrderValidationException($"Order validation failed: {errors}");
            }
            
            // Calculate order totals
            var calculation = await CalculateOrderTotalsAsync(
                request.Items, 
                request.DiscountPercentage, 
                request.DiscountAmount);
            
            // Update order properties
            order.CustomerID = request.CustomerId;
            order.ServiceTypeID = (byte)request.ServiceType;
            order.ServiceType = request.ServiceType.ToString();
            order.TableNumber = request.TableNumber;
            order.Status = request.Status.ToString();
            order.Subtotal = calculation.Subtotal;
            order.TaxAmount = calculation.TaxAmount;
            order.TotalCost = calculation.TotalAmount;
            order.TotalAmount = calculation.TotalAmount;
            order.DiscountPercentage = request.DiscountPercentage;
            order.DiscountAmount = calculation.DiscountAmount;
            order.VoucherID = request.VoucherId;
            order.InvoiceNote = request.Notes;
            order.Notes = request.Notes;
            order.IsInvoiceNotePrintable = request.IsNotesPrintable;
            order.UpdatedAt = DateTime.UtcNow;
            
            // Update order items (remove old, add new)
            order.Items.Clear();
            foreach (var itemDto in request.Items)
            {
                var orderItem = new OrderItem
                {
                    InvoiceID = order.ID, // Legacy: InvoiceID is OrderID
                    CategoryItemID = itemDto.ProductId, // Legacy: CategoryItemID is ProductID
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    Notes = itemDto.Notes
                };
                order.Items.Add(orderItem);
            }
            
            // Save changes
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order updated successfully: {OrderId}", order.ID);
            
            // Audit log
            await _auditLogService.LogApiRequestAsync(
                userId: userId,
                action: "UpdateOrder",
                requestPath: $"/api/orders/{request.OrderId}",
                requestMethod: "PUT",
                statusCode: 200,
                duration: 0);
            
            // Return DTO
            return await GetOrderByIdAsync(order.ID) 
                ?? throw new InvalidOperationException("Failed to retrieve updated order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            _logger.LogDebug("Getting order by ID: {OrderId}", orderId);
            
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", orderId);
                return null;
            }
            
            return MapToDto(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by ID: {OrderId}", orderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderDto>> GetPendingOrdersAsync(int? userId = null, byte? tableNumber = null)
    {
        try
        {
            _logger.LogInformation("Getting pending orders (userId: {UserId}, tableNumber: {TableNumber})", 
                userId, tableNumber);
            
            var orders = await _unitOfWork.Orders.GetPendingOrdersAsync();
            
            // Filter by user if specified
            if (userId.HasValue)
            {
                orders = orders.Where(o => o.UserID == userId.Value);
            }
            
            // Filter by table number if specified
            if (tableNumber.HasValue)
            {
                orders = orders.Where(o => o.TableNumber == tableNumber.Value);
            }
            
            var orderDtos = orders.Select(MapToDto).ToList();
            
            _logger.LogInformation("Found {Count} pending orders", orderDtos.Count);
            
            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending orders");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId, int limit = 20)
    {
        try
        {
            _logger.LogDebug("Getting orders for customer {CustomerId} (limit: {Limit})", customerId, limit);
            
            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId, limit);
            
            var orderDtos = orders.Select(MapToDto).ToList();
            
            _logger.LogDebug("Found {Count} orders for customer {CustomerId}", orderDtos.Count, customerId);
            
            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderDto>> GetTodaysOrdersAsync(int? userId = null)
    {
        try
        {
            _logger.LogInformation("Getting today's orders (userId: {UserId})", userId);
            
            var orders = await _unitOfWork.Orders.GetTodaysOrdersAsync();
            
            // Filter by user if specified
            if (userId.HasValue)
            {
                orders = orders.Where(o => o.UserID == userId.Value);
            }
            
            var orderDtos = orders.Select(MapToDto).ToList();
            
            _logger.LogInformation("Found {Count} orders for today", orderDtos.Count);
            
            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's orders");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate, int? userId = null)
    {
        try
        {
            _logger.LogInformation("Getting orders from {FromDate} to {ToDate} (userId: {UserId})", 
                fromDate, toDate, userId);
            
            var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(fromDate, toDate);
            
            // Filter by user if specified
            if (userId.HasValue)
            {
                orders = orders.Where(o => o.UserID == userId.Value);
            }
            
            var orderDtos = orders.Select(MapToDto).ToList();
            
            _logger.LogInformation("Found {Count} orders in date range", orderDtos.Count);
            
            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by date range");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderDto>> SplitOrderAsync(int orderId, List<CreateOrderRequest> splitRequests, int userId)
    {
        try
        {
            _logger.LogInformation("Splitting order {OrderId} into {Count} orders", orderId, splitRequests.Count);
            
            // Get original order
            var originalOrder = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            if (originalOrder == null)
            {
                throw new OrderNotFoundException($"Order {orderId} not found");
            }
            
            // Validate that split requests contain all items from original order
            var originalItemCount = originalOrder.Items.Sum(i => i.Quantity);
            var splitItemCount = splitRequests.SelectMany(r => r.Items).Sum(i => i.Quantity);
            
            if (originalItemCount != splitItemCount)
            {
                throw new OrderValidationException(
                    $"Split orders must contain all items from original order. " +
                    $"Original: {originalItemCount}, Split: {splitItemCount}");
            }
            
            var splitOrders = new List<OrderDto>();
            
            // Create split orders
            foreach (var splitRequest in splitRequests)
            {
                var splitOrder = await CreateOrderAsync(splitRequest, userId);
                splitOrders.Add(splitOrder);
            }
            
            // Mark original order as split/canceled
            originalOrder.Status = "Split";
            originalOrder.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(originalOrder);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order {OrderId} split successfully into {Count} orders", 
                orderId, splitOrders.Count);
            
            return splitOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting order {OrderId}", orderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelOrderAsync(int orderId, int userId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Canceling order {OrderId} by user {UserId}", orderId, userId);
            
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", orderId);
                return false;
            }
            
            // Update order status
            order.Status = "Canceled";
            order.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                order.Notes = $"{order.Notes}\nCancellation reason: {reason}";
            }
            
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order {OrderId} canceled successfully", orderId);
            
            // Audit log
            await _auditLogService.LogApiRequestAsync(
                userId: userId,
                action: "CancelOrder",
                requestPath: $"/api/orders/{orderId}/cancel",
                requestMethod: "POST",
                statusCode: 200,
                duration: 0);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling order {OrderId}", orderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<OrderDto> CompleteOrderAsync(int orderId, int userId)
    {
        try
        {
            _logger.LogInformation("Completing order {OrderId} by user {UserId}", orderId, userId);
            
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                throw new OrderNotFoundException($"Order {orderId} not found");
            }
            
            // Update order status
            order.Status = OrderStatus.Completed.ToString();
            order.CompletedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Order {OrderId} completed successfully", orderId);
            
            // Audit log
            await _auditLogService.LogApiRequestAsync(
                userId: userId,
                action: "CompleteOrder",
                requestPath: $"/api/orders/{orderId}/complete",
                requestMethod: "POST",
                statusCode: 200,
                duration: 0);
            
            return MapToDto(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing order {OrderId}", orderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<OrderValidationResult> ValidateOrderItemsAsync(List<OrderItemDto> items)
    {
        var result = new OrderValidationResult { IsValid = true };
        
        try
        {
            if (items == null || items.Count == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Order must contain at least one item");
                return result;
            }
            
            foreach (var item in items)
            {
                // Validate product exists
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Product {item.ProductId} not found");
                    result.ItemErrors[item.ProductId] = "Product not found";
                    continue;
                }
                
                // Validate product is available
                if (!product.IsAvailable)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Product '{product.Name}' is not available");
                    result.ItemErrors[item.ProductId] = "Product not available";
                    continue;
                }
                
                // Validate stock availability
                var stockAvailable = await _productService.CheckStockAvailabilityAsync(
                    item.ProductId, item.Quantity);
                    
                if (!stockAvailable)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Insufficient stock for product '{product.Name}'");
                    result.ItemErrors[item.ProductId] = "Insufficient stock";
                    continue;
                }
                
                // Validate quantity
                if (item.Quantity <= 0)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid quantity for product '{product.Name}'");
                    result.ItemErrors[item.ProductId] = "Invalid quantity";
                }
                
                // Validate price matches product price
                if (item.UnitPrice != product.Price)
                {
                    _logger.LogWarning(
                        "Price mismatch for product {ProductId}: Expected {Expected}, Got {Actual}",
                        item.ProductId, product.Price, item.UnitPrice);
                    // Update to correct price
                    item.UnitPrice = product.Price;
                    item.TotalPrice = product.Price * item.Quantity;
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating order items");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    /// <inheritdoc />
    public async Task<OrderCalculationResult> CalculateOrderTotalsAsync(
        List<OrderItemDto> items, 
        decimal? discountPercentage = null, 
        decimal? discountAmount = null)
    {
        try
        {
            // Calculate subtotal
            var subtotal = items.Sum(i => i.TotalPrice);
            
            // Calculate discount
            decimal discount = 0;
            if (discountPercentage.HasValue && discountPercentage.Value > 0)
            {
                discount = subtotal * (discountPercentage.Value / 100);
            }
            else if (discountAmount.HasValue && discountAmount.Value > 0)
            {
                discount = discountAmount.Value;
            }
            
            // Ensure discount doesn't exceed subtotal
            if (discount > subtotal)
            {
                discount = subtotal;
            }
            
            // Calculate subtotal after discount
            var subtotalAfterDiscount = subtotal - discount;
            
            // Calculate tax
            var tax = subtotalAfterDiscount * TaxRate;
            
            // Calculate total
            var total = subtotalAfterDiscount + tax;
            
            return new OrderCalculationResult
            {
                Subtotal = subtotal,
                TaxAmount = tax,
                DiscountAmount = discount,
                TotalAmount = total
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating order totals");
            throw;
        }
    }

    /// <summary>
    /// Map Order entity to OrderDto
    /// </summary>
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.ID,
            CustomerId = order.CustomerID,
            Customer = order.Customer != null ? new CustomerDto
            {
                Id = order.Customer.ID,
                Name = order.Customer.Name,
                Telephone = order.Customer.Telephone,
                Email = order.Customer.Email
            } : null,
            UserId = order.UserID,
            ServiceType = Enum.Parse<ServiceType>(order.ServiceType),
            TableNumber = order.TableNumber,
            Status = Enum.Parse<OrderStatus>(order.Status ?? "Pending"),
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.ID,
                ProductId = i.CategoryItemID, // Legacy: CategoryItemID is ProductID
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Notes = i.Notes
            }).ToList(),
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountPercentage = order.DiscountPercentage,
            DiscountAmount = order.DiscountAmount,
            VoucherId = order.VoucherID,
            TotalAmount = order.TotalAmount,
            AmountPaid = order.AmountPaid,
            ChangeAmount = order.ChangeAmount,
            Notes = order.Notes,
            IsNotesPrintable = order.IsInvoiceNotePrintable,
            ScheduledTime = order.ScheduledTime,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CompletedAt = order.CompletedAt
        };
    }
}

/// <summary>
/// Exception thrown when order validation fails
/// </summary>
public class OrderValidationException : Exception
{
    public OrderValidationException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when order is not found
/// </summary>
public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when order is locked by another user
/// </summary>
public class OrderLockedException : Exception
{
    public OrderLockedException(string message) : base(message) { }
}
