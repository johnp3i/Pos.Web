# Task 4.4 Completion Summary: Specialized Repositories for Web Schema

**Task**: Create specialized repositories for web schema entities  
**Status**: ✅ Completed  
**Date**: 2026-03-05  
**Requirements**: US-6.2, FR-3, NFR-4

---

## Overview

Successfully implemented specialized repositories for web schema entities (OrderLock, ApiAuditLog, FeatureFlag, SyncQueue) following JDS repository design guidelines. Each repository provides domain-specific methods beyond the generic CRUD operations, enabling efficient data access patterns for the web-based POS system.

**IMPORTANT NOTE**: UserSession entity was initially created but removed during implementation as it belongs to the WebPosMembershipDbContext (membership database) rather than the PosDbContext (POS database). The existing SessionManager service correctly uses the membership database for session management.

---

## Files Created

### 1. OrderLock Repository
**Interface**: `Pos.Web/Pos.Web.Infrastructure/Repositories/IOrderLockRepository.cs`  
**Implementation**: `Pos.Web/Pos.Web.Infrastructure/Repositories/OrderLockRepository.cs`

**Domain-Specific Methods**:
- `GetActiveLockByOrderIdAsync(int orderId)` - Get active lock for a specific order
- `GetActiveLocksByUserIdAsync(int userId)` - Get all active locks for a user
- `GetExpiredLocksAsync()` - Get all expired locks that need cleanup
- `ReleaseLockAsync(int orderId, int userId)` - Release lock for an order
- `ExtendLockAsync(int orderId, int userId, int additionalMinutes)` - Extend lock expiration
- `CleanupExpiredLocksAsync()` - Cleanup expired locks (set IsActive to false)

**Use Cases**:
- Prevent concurrent editing of orders (US-6.2)
- Display "locked by user" indicators in UI
- Automatic lock expiration and cleanup
- Lock extension for long-running operations

---

### 2. AuditLog Repository
**Interface**: `Pos.Web/Pos.Web.Infrastructure/Repositories/IAuditLogRepository.cs`  
**Implementation**: `Pos.Web/Pos.Web.Infrastructure/Repositories/AuditLogRepository.cs`

**Domain-Specific Methods**:
- `GetByUserIdAsync(int userId, int pageNumber, int pageSize)` - Get audit logs for a user with pagination
- `GetByEntityAsync(string entityType, int entityId)` - Get audit logs for a specific entity
- `GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)` - Get logs within date range
- `GetErrorLogsAsync(int pageNumber, int pageSize)` - Get error logs (4xx and 5xx status codes)
- `GetByActionAsync(string action, int pageNumber, int pageSize)` - Get logs by action type
- `DeleteOldLogsAsync(int daysToKeep)` - Delete old audit logs
- `GetStatisticsAsync(DateTime startDate, DateTime endDate)` - Get audit log statistics

**Statistics Returned**:
- Total requests
- Successful requests
- Error requests
- Average/Max/Min duration
- Unique users
- Top actions
- Errors by status code

**Use Cases**:
- Track all API operations (FR-3)
- Security auditing and compliance
- Performance monitoring
- Error tracking and debugging
- User activity monitoring

---

### 3. FeatureFlag Repository
**Interface**: `Pos.Web/Pos.Web.Infrastructure/Repositories/IFeatureFlagRepository.cs`  
**Implementation**: `Pos.Web/Pos.Web.Infrastructure/Repositories/FeatureFlagRepository.cs`

**Domain-Specific Methods**:
- `GetByNameAsync(string name)` - Get feature flag by name
- `GetEnabledFlagsAsync()` - Get all enabled feature flags
- `IsFeatureEnabledAsync(string name)` - Check if feature is enabled globally
- `IsFeatureEnabledForUserAsync(string name, int userId)` - Check if enabled for specific user
- `IsFeatureEnabledForRoleAsync(string name, string role)` - Check if enabled for specific role
- `EnableFeatureAsync(string name, int updatedBy)` - Enable a feature flag
- `DisableFeatureAsync(string name, int updatedBy)` - Disable a feature flag
- `UpdateUserRestrictionsAsync(string name, int[]? userIds, int updatedBy)` - Update user restrictions
- `UpdateRoleRestrictionsAsync(string name, string[]? roles, int updatedBy)` - Update role restrictions

**Use Cases**:
- Dynamic feature toggling (US-9.2)
- A/B testing
- Gradual feature rollout
- User-specific and role-specific features
- Feature access control

---

### 4. SyncQueue Repository
**Interface**: `Pos.Web/Pos.Web.Infrastructure/Repositories/ISyncQueueRepository.cs`  
**Implementation**: `Pos.Web/Pos.Web.Infrastructure/Repositories/SyncQueueRepository.cs`

**Domain-Specific Methods**:
- `GetPendingByDeviceIdAsync(string deviceId)` - Get pending operations for a device
- `GetPendingByUserIdAsync(int userId)` - Get pending operations for a user
- `GetPendingOperationsAsync(int limit)` - Get all pending operations with limit
- `GetFailedOperationsAsync(int maxAttempts)` - Get failed operations that need retry
- `GetByEntityAsync(string entityType, int entityId)` - Get operations by entity
- `MarkAsProcessingAsync(long id)` - Mark operation as processing
- `MarkAsCompletedAsync(long id)` - Mark operation as completed
- `MarkAsFailedAsync(long id, string errorMessage)` - Mark operation as failed
- `RetryOperationAsync(long id)` - Retry failed operation
- `DeleteCompletedOperationsAsync(int daysToKeep)` - Delete old completed operations
- `GetStatisticsAsync()` - Get sync queue statistics

**Statistics Returned**:
- Pending count
- Processing count
- Completed count
- Failed count
- Total count

**Use Cases**:
- Offline operation queuing (US-7.1)
- Automatic sync when online
- Retry failed operations
- Conflict resolution
- Sync monitoring and statistics

---

## Updated Files

### UnitOfWork Interface
**Path**: `Pos.Web/Pos.Web.Infrastructure/UnitOfWork/IUnitOfWork.cs`

**Changes**:
- Updated repository accessors from generic `IRepository<T>` to specialized interfaces
- Changed `IRepository<OrderLock>` to `IOrderLockRepository`
- Changed `IRepository<FeatureFlag>` to `IFeatureFlagRepository`
- Changed `IRepository<SyncQueue>` to `ISyncQueueRepository`
- Changed `IRepository<ApiAuditLog>` to `IAuditLogRepository`
- **REMOVED**: UserSession repository accessor (belongs to membership database)

### UnitOfWork Implementation
**Path**: `Pos.Web/Pos.Web.Infrastructure/UnitOfWork/UnitOfWork.cs`

**Changes**:
- Updated private field types to specialized repository interfaces
- Updated lazy initialization to create specialized repository instances
- Changed `new GenericRepository<OrderLock>()` to `new OrderLockRepository()`
- Changed `new GenericRepository<FeatureFlag>()` to `new FeatureFlagRepository()`
- Changed `new GenericRepository<SyncQueue>()` to `new SyncQueueRepository()`
- Changed `new GenericRepository<ApiAuditLog>()` to `new AuditLogRepository()`
- **REMOVED**: UserSession repository initialization (belongs to membership database)

---

## UserSession Resolution

During implementation, it was discovered that there are **two different UserSession tables** in the system:

1. **`dbo.UserSessions`** in the **WebPosMembership database** - Used by the existing authentication system
   - References `AspNetUsers` table (string ID)
   - Managed by `WebPosMembershipDbContext`
   - Used by existing `SessionManager` service

2. **`web.UserSessions`** in the **POS database** - Initially planned for web schema
   - Would reference `dbo.Users` table (int ID)
   - Would be managed by `PosDbContext`

**Resolution**: The UserSession entity and repository were **removed from the POS database context** because:
- The existing `SessionManager` service already handles session management correctly
- It uses the `WebPosMembershipDbContext` which is the proper location for authentication-related data
- The membership database is specifically designed for user authentication and session management
- Duplicating session management would create confusion and potential conflicts

**Impact**: 
- ✅ No functionality lost - session management continues to work through existing `SessionManager`
- ✅ Cleaner separation of concerns - authentication stays in membership database
- ✅ No conflicts between different UserSession implementations
- ✅ Maintains existing authentication architecture

---

## JDS Compliance Checklist

✅ **Async/await throughout** - All methods are asynchronous  
✅ **Try/catch with rethrow** - All methods follow this pattern  
✅ **No logging in repositories** - Logging handled by service layer  
✅ **Strong typing** - All repositories are strongly typed  
✅ **Null-safe parameters** - Using `?? (object)DBNull.Value` where needed  
✅ **Proper resource disposal** - DbContext managed by DI container  
✅ **Include() for navigation properties** - Eager loading to prevent N+1 queries  
✅ **Pagination support** - Methods with large result sets support pagination

---

## Implementation Patterns

### 1. Eager Loading Pattern
All repositories use `Include()` to load navigation properties:

```csharp
return await _dbSet
    .Include(x => x.User)
    .FirstOrDefaultAsync(x => x.ID == id);
```

**Benefits**:
- Prevents N+1 query problems
- Single database round-trip
- Better performance

### 2. Pagination Pattern
Methods returning large result sets support pagination:

```csharp
public async Task<IEnumerable<ApiAuditLog>> GetByUserIdAsync(
    int userId, 
    int pageNumber = 1, 
    int pageSize = 50)
{
    return await _dbSet
        .Where(x => x.UserID == userId)
        .OrderByDescending(x => x.Timestamp)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}
```

**Benefits**:
- Reduces memory usage
- Improves response times
- Better user experience

### 3. Null-Safe Parameter Pattern
Methods handle null parameters safely:

```csharp
public async Task<bool> UpdateUserRestrictionsAsync(
    string name, 
    int[]? userIds, 
    int updatedBy)
{
    flag.EnabledForUserIDs = userIds != null && userIds.Length > 0
        ? JsonSerializer.Serialize(userIds)
        : null;
}
```

**Benefits**:
- Prevents null reference exceptions
- Follows JDS guideline
- Consistent behavior

### 4. Batch Update Pattern
Methods that update multiple records use batch operations:

```csharp
public async Task<int> CleanupExpiredLocksAsync()
{
    var expiredLocks = await _dbSet
        .Where(x => x.IsActive && x.LockExpiresAt <= DateTime.UtcNow)
        .ToListAsync();

    foreach (var lockEntity in expiredLocks)
    {
        lockEntity.IsActive = false;
        lockEntity.UpdatedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
    return expiredLocks.Count;
}
```

**Benefits**:
- Single database transaction
- Better performance
- Atomic operation

---

## Usage Examples

### 1. Order Locking Service

```csharp
public class OrderLockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderLockService> _logger;
    
    public async Task<bool> AcquireLockAsync(int orderId, int userId, string sessionId)
    {
        // Check if order is already locked
        var existingLock = await _unitOfWork.OrderLocks
            .GetActiveLockByOrderIdAsync(orderId);
        
        if (existingLock != null)
        {
            _logger.LogWarning("Order {OrderId} is already locked by user {UserId}", 
                orderId, existingLock.UserID);
            return false;
        }
        
        // Create new lock
        var lockEntity = new OrderLock
        {
            OrderID = orderId,
            UserID = userId,
            SessionID = sessionId,
            LockAcquiredAt = DateTime.UtcNow,
            LockExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.OrderLocks.AddAsync(lockEntity);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Lock acquired for order {OrderId} by user {UserId}", 
            orderId, userId);
        return true;
    }
    
    public async Task<bool> ReleaseLockAsync(int orderId, int userId)
    {
        var released = await _unitOfWork.OrderLocks.ReleaseLockAsync(orderId, userId);
        
        if (released)
        {
            _logger.LogInformation("Lock released for order {OrderId} by user {UserId}", 
                orderId, userId);
        }
        
        return released;
    }
}
```

### 2. Audit Logging Service

```csharp
public class AuditService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task LogApiCallAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        HttpContext httpContext,
        int statusCode,
        int duration)
    {
        var auditLog = new ApiAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserID = userId,
            Action = action,
            EntityType = entityType,
            EntityID = entityId,
            IPAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
            RequestPath = httpContext.Request.Path,
            RequestMethod = httpContext.Request.Method,
            StatusCode = statusCode,
            Duration = duration
        };
        
        await _unitOfWork.ApiAuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<Dictionary<string, object>> GetDailyStatisticsAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = date.Date.AddDays(1);
        
        return await _unitOfWork.ApiAuditLogs
            .GetStatisticsAsync(startDate, endDate);
    }
}
```

### 3. Feature Flag Service

```csharp
public class FeatureFlagService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    
    public async Task<bool> IsFeatureEnabledAsync(string featureName, int userId, string role)
    {
        // Check cache first
        var cacheKey = $"feature:{featureName}:{userId}:{role}";
        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }
        
        // Check database
        var isEnabled = await _unitOfWork.FeatureFlags
            .IsFeatureEnabledForUserAsync(featureName, userId);
        
        if (!isEnabled)
        {
            isEnabled = await _unitOfWork.FeatureFlags
                .IsFeatureEnabledForRoleAsync(featureName, role);
        }
        
        // Cache result for 5 minutes
        _cache.Set(cacheKey, isEnabled, TimeSpan.FromMinutes(5));
        
        return isEnabled;
    }
    
    public async Task<bool> EnableFeatureForUsersAsync(
        string featureName, 
        int[] userIds, 
        int updatedBy)
    {
        var updated = await _unitOfWork.FeatureFlags
            .UpdateUserRestrictionsAsync(featureName, userIds, updatedBy);
        
        if (updated)
        {
            // Invalidate cache
            _cache.Remove($"feature:{featureName}");
        }
        
        return updated;
    }
}
```

### 4. Offline Sync Service

```csharp
public class SyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncService> _logger;
    
    public async Task ProcessPendingOperationsAsync(int batchSize = 100)
    {
        var pendingOps = await _unitOfWork.SyncQueues
            .GetPendingOperationsAsync(batchSize);
        
        foreach (var operation in pendingOps)
        {
            await _unitOfWork.SyncQueues.MarkAsProcessingAsync(operation.ID);
            
            try
            {
                // Process operation based on type
                await ProcessOperationAsync(operation);
                
                await _unitOfWork.SyncQueues.MarkAsCompletedAsync(operation.ID);
                
                _logger.LogInformation("Sync operation {Id} completed", operation.ID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.SyncQueues.MarkAsFailedAsync(
                    operation.ID, 
                    ex.Message);
                
                _logger.LogError(ex, "Sync operation {Id} failed", operation.ID);
            }
        }
    }
    
    public async Task RetryFailedOperationsAsync()
    {
        var failedOps = await _unitOfWork.SyncQueues.GetFailedOperationsAsync(maxAttempts: 3);
        
        foreach (var operation in failedOps)
        {
            await _unitOfWork.SyncQueues.RetryOperationAsync(operation.ID);
            _logger.LogInformation("Retrying sync operation {Id}", operation.ID);
        }
    }
}
```

---

## Background Services

### 1. Lock Cleanup Service

```csharp
public class LockCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LockCleanupService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                var cleanedCount = await unitOfWork.OrderLocks.CleanupExpiredLocksAsync();
                
                if (cleanedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired locks", cleanedCount);
                }
                
                // Run every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired locks");
            }
        }
    }
}
```

### 2. Session Cleanup Service

```csharp
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                // Cleanup expired sessions
                var expiredCount = await unitOfWork.UserSessions.CleanupExpiredSessionsAsync();
                
                // Delete old inactive sessions (older than 30 days)
                var deletedCount = await unitOfWork.UserSessions.DeleteOldSessionsAsync(30);
                
                _logger.LogInformation(
                    "Session cleanup: {ExpiredCount} expired, {DeletedCount} deleted", 
                    expiredCount, deletedCount);
                
                // Run every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up sessions");
            }
        }
    }
}
```

### 3. Audit Log Archival Service

```csharp
public class AuditLogArchivalService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogArchivalService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                // Delete audit logs older than 90 days
                var deletedCount = await unitOfWork.ApiAuditLogs.DeleteOldLogsAsync(90);
                
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Archived {Count} old audit logs", deletedCount);
                }
                
                // Run daily at 2 AM
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var delay = nextRun - now;
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving audit logs");
            }
        }
    }
}
```

---

## Testing Recommendations

### Unit Tests

```csharp
[TestClass]
public class OrderLockRepositoryTests
{
    private PosDbContext _context;
    private IOrderLockRepository _repository;
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        
        _context = new PosDbContext(options);
        _repository = new OrderLockRepository(_context);
    }
    
    [TestMethod]
    public async Task GetActiveLockByOrderId_ReturnsLock_WhenExists()
    {
        // Arrange
        var lockEntity = new OrderLock
        {
            OrderID = 1,
            UserID = 1,
            LockAcquiredAt = DateTime.UtcNow,
            LockExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(lockEntity);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetActiveLockByOrderIdAsync(1);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.OrderID);
        Assert.IsTrue(result.IsActive);
    }
    
    [TestMethod]
    public async Task ReleaseLock_SetsIsActiveToFalse()
    {
        // Arrange
        var lockEntity = new OrderLock
        {
            OrderID = 1,
            UserID = 1,
            LockAcquiredAt = DateTime.UtcNow,
            LockExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(lockEntity);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.ReleaseLockAsync(1, 1);
        
        // Assert
        Assert.IsTrue(result);
        var lock = await _repository.GetByIdAsync(lockEntity.ID);
        Assert.IsFalse(lock.IsActive);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }
}
```

---

## Benefits

### 1. Domain-Specific Operations
- Each repository provides methods tailored to its entity's use cases
- Reduces code duplication in service layer
- Encapsulates complex queries

### 2. Performance Optimization
- Eager loading with Include() prevents N+1 queries
- Pagination support for large result sets
- Efficient batch operations

### 3. Maintainability
- Clear separation of concerns
- Easy to test with mocking
- Consistent patterns across repositories

### 4. Type Safety
- Strongly typed interfaces
- Compile-time checking
- IntelliSense support

---

## Next Steps

### Task 5: Infrastructure Services
- Implement Redis caching service (Task 5.1)
- Implement feature flag service (Task 5.2)
- Implement audit logging service (Task 5.3)

### Integration
- Register repositories in DI container
- Create background services for cleanup tasks
- Implement service layer using repositories

---

## Validation

### Code Compilation
✅ No compilation errors  
✅ No warnings  
✅ All dependencies resolved

### JDS Guidelines
✅ Async/await pattern used throughout  
✅ Try/catch with rethrow pattern  
✅ No logging in repository layer  
✅ Strong typing  
✅ Null-safe parameters  
✅ Proper resource disposal

### Requirements Validation
✅ **US-6.2 (Order Locking)**: OrderLockRepository supports concurrent editing prevention  
✅ **FR-3 (Data Integrity)**: All repositories use proper error handling  
✅ **NFR-4 (Maintainability)**: Clean architecture with specialized repositories

---

## Summary

Task 4.4 has been successfully completed with:

1. ✅ 4 specialized repository interfaces created (OrderLock, AuditLog, FeatureFlag, SyncQueue)
2. ✅ 4 specialized repository implementations created
3. ✅ Unit of Work updated to use specialized repositories
4. ✅ UserSession conflict resolved (removed from POS context, stays in membership context)
5. ✅ All repositories follow JDS guidelines
6. ✅ Domain-specific methods for each entity
7. ✅ Pagination support for large result sets
8. ✅ Eager loading to prevent N+1 queries
9. ✅ No compilation errors
10. ✅ Ready for service layer integration

The specialized repositories provide a solid foundation for the infrastructure layer, enabling efficient data access patterns and supporting the requirements of the web-based POS system. The UserSession conflict has been properly resolved by maintaining the existing authentication architecture.

**Status**: ✅ **COMPLETE**
