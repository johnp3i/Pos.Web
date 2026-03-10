using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.UnitOfWork;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Order lock service implementation for managing order locking
/// Prevents concurrent editing of orders with automatic expiration
/// </summary>
public class OrderLockService : IOrderLockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiAuditLogService _auditLogService;
    private readonly ILogger<OrderLockService> _logger;

    // Default lock timeout in minutes
    private const int DefaultLockTimeoutMinutes = 15;
    private const int MaxLockTimeoutMinutes = 60;

    public OrderLockService(
        IUnitOfWork unitOfWork,
        IApiAuditLogService auditLogService,
        ILogger<OrderLockService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrderLockResultDto> AcquireLockAsync(
        int orderId, 
        int userId, 
        string? sessionId = null, 
        string? deviceInfo = null, 
        int timeoutMinutes = DefaultLockTimeoutMinutes)
    {
        try
        {
            _logger.LogInformation("Attempting to acquire lock on order {OrderId} for user {UserId}", orderId, userId);

            // Validate timeout
            if (timeoutMinutes <= 0 || timeoutMinutes > MaxLockTimeoutMinutes)
            {
                timeoutMinutes = DefaultLockTimeoutMinutes;
            }

            // Check for existing active lock
            var existingLock = await _unitOfWork.OrderLocks.GetActiveLockByOrderIdAsync(orderId);

            if (existingLock != null)
            {
                // Check if the lock is owned by the same user
                if (existingLock.UserID == userId)
                {
                    // Extend the existing lock
                    _logger.LogInformation("User {UserId} already holds lock on order {OrderId}, extending", userId, orderId);
                    
                    existingLock.LockExpiresAt = DateTime.UtcNow.AddMinutes(timeoutMinutes);
                    existingLock.UpdatedAt = DateTime.UtcNow;
                    
                    await _unitOfWork.OrderLocks.UpdateAsync(existingLock);
                    await _unitOfWork.SaveChangesAsync();

                    return new OrderLockResultDto
                    {
                        Success = true,
                        LockId = existingLock.ID,
                        ExpiresAt = existingLock.LockExpiresAt
                    };
                }

                // Lock is held by another user
                _logger.LogWarning("Order {OrderId} is already locked by user {LockedByUserId}", orderId, existingLock.UserID);
                
                return new OrderLockResultDto
                {
                    Success = false,
                    ErrorMessage = $"Order is currently being edited by {existingLock.User?.FullName ?? "another user"}",
                    ExistingLock = MapToStatusDto(existingLock)
                };
            }

            // Create new lock
            var newLock = new OrderLock
            {
                OrderID = orderId,
                UserID = userId,
                LockAcquiredAt = DateTime.UtcNow,
                LockExpiresAt = DateTime.UtcNow.AddMinutes(timeoutMinutes),
                IsActive = true,
                SessionID = sessionId,
                DeviceInfo = deviceInfo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OrderLocks.AddAsync(newLock);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Lock acquired successfully on order {OrderId} for user {UserId}, expires at {ExpiresAt}", 
                orderId, userId, newLock.LockExpiresAt);

            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "AcquireLock",
                entityType: "OrderLock",
                entityId: newLock.ID,
                newValues: System.Text.Json.JsonSerializer.Serialize(new { orderId, timeoutMinutes, sessionId, deviceInfo }));

            return new OrderLockResultDto
            {
                Success = true,
                LockId = newLock.ID,
                ExpiresAt = newLock.LockExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock on order {OrderId} for user {UserId}", orderId, userId);
            throw new ServiceException($"Failed to acquire lock on order {orderId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ReleaseLockAsync(int orderId, int userId)
    {
        try
        {
            _logger.LogInformation("Releasing lock on order {OrderId} for user {UserId}", orderId, userId);

            var result = await _unitOfWork.OrderLocks.ReleaseLockAsync(orderId, userId);

            if (result)
            {
                _logger.LogInformation("Lock released successfully on order {OrderId} for user {UserId}", orderId, userId);

                // Log audit
                await _auditLogService.LogEntityChangeAsync(
                    userId: userId,
                    action: "ReleaseLock",
                    entityType: "OrderLock",
                    entityId: orderId,
                    newValues: System.Text.Json.JsonSerializer.Serialize(new { orderId, userId }));
            }
            else
            {
                _logger.LogWarning("Failed to release lock on order {OrderId} for user {UserId} - lock not found or not owned", 
                    orderId, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock on order {OrderId} for user {UserId}", orderId, userId);
            throw new ServiceException($"Failed to release lock on order {orderId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<OrderLockStatusDto> GetLockStatusAsync(int orderId)
    {
        try
        {
            _logger.LogDebug("Getting lock status for order {OrderId}", orderId);

            var activeLock = await _unitOfWork.OrderLocks.GetActiveLockByOrderIdAsync(orderId);

            if (activeLock == null)
            {
                return new OrderLockStatusDto
                {
                    IsLocked = false,
                    OrderId = orderId
                };
            }

            return MapToStatusDto(activeLock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lock status for order {OrderId}", orderId);
            throw new ServiceException($"Failed to get lock status for order {orderId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExtendLockAsync(int orderId, int userId, int additionalMinutes = DefaultLockTimeoutMinutes)
    {
        try
        {
            _logger.LogInformation("Extending lock on order {OrderId} for user {UserId} by {Minutes} minutes", 
                orderId, userId, additionalMinutes);

            // Validate additional minutes
            if (additionalMinutes <= 0 || additionalMinutes > MaxLockTimeoutMinutes)
            {
                additionalMinutes = DefaultLockTimeoutMinutes;
            }

            var result = await _unitOfWork.OrderLocks.ExtendLockAsync(orderId, userId, additionalMinutes);

            if (result)
            {
                _logger.LogInformation("Lock extended successfully on order {OrderId} for user {UserId}", orderId, userId);

                // Log audit
                await _auditLogService.LogEntityChangeAsync(
                    userId: userId,
                    action: "ExtendLock",
                    entityType: "OrderLock",
                    entityId: orderId,
                    newValues: System.Text.Json.JsonSerializer.Serialize(new { orderId, userId, additionalMinutes }));
            }
            else
            {
                _logger.LogWarning("Failed to extend lock on order {OrderId} for user {UserId} - lock not found or expired", 
                    orderId, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending lock on order {OrderId} for user {UserId}", orderId, userId);
            throw new ServiceException($"Failed to extend lock on order {orderId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ForceReleaseLockAsync(int orderId, int adminUserId)
    {
        try
        {
            _logger.LogWarning("Force releasing lock on order {OrderId} by admin user {AdminUserId}", orderId, adminUserId);

            var activeLock = await _unitOfWork.OrderLocks.GetActiveLockByOrderIdAsync(orderId);

            if (activeLock == null)
            {
                _logger.LogWarning("No active lock found on order {OrderId} for force release", orderId);
                return false;
            }

            var originalUserId = activeLock.UserID;

            activeLock.IsActive = false;
            activeLock.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OrderLocks.UpdateAsync(activeLock);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("Lock force released on order {OrderId} by admin {AdminUserId}, original user was {OriginalUserId}", 
                orderId, adminUserId, originalUserId);

            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: adminUserId,
                action: "ForceReleaseLock",
                entityType: "OrderLock",
                entityId: activeLock.ID,
                newValues: System.Text.Json.JsonSerializer.Serialize(new { orderId, adminUserId, originalUserId }));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force releasing lock on order {OrderId} by admin {AdminUserId}", orderId, adminUserId);
            throw new ServiceException($"Failed to force release lock on order {orderId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<OrderLockStatusDto>> GetUserLocksAsync(int userId)
    {
        try
        {
            _logger.LogDebug("Getting active locks for user {UserId}", userId);

            var locks = await _unitOfWork.OrderLocks.GetActiveLocksByUserIdAsync(userId);

            return locks.Select(MapToStatusDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active locks for user {UserId}", userId);
            throw new ServiceException($"Failed to get active locks for user {userId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredLocksAsync()
    {
        try
        {
            _logger.LogInformation("Starting cleanup of expired locks");

            var count = await _unitOfWork.OrderLocks.CleanupExpiredLocksAsync();

            _logger.LogInformation("Cleaned up {Count} expired locks", count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired locks");
            throw new ServiceException("Failed to cleanup expired locks", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsLockedByAnotherUserAsync(int orderId, int userId)
    {
        try
        {
            var activeLock = await _unitOfWork.OrderLocks.GetActiveLockByOrderIdAsync(orderId);

            if (activeLock == null)
            {
                return false;
            }

            return activeLock.UserID != userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if order {OrderId} is locked by another user", orderId);
            throw new ServiceException($"Failed to check lock status for order {orderId}", ex);
        }
    }

    /// <summary>
    /// Map OrderLock entity to OrderLockStatusDto
    /// </summary>
    private static OrderLockStatusDto MapToStatusDto(OrderLock lockEntity)
    {
        return new OrderLockStatusDto
        {
            IsLocked = true,
            LockId = lockEntity.ID,
            OrderId = lockEntity.OrderID,
            LockedByUserId = lockEntity.UserID,
            LockedByUserName = lockEntity.User?.FullName,
            LockAcquiredAt = lockEntity.LockAcquiredAt,
            LockExpiresAt = lockEntity.LockExpiresAt,
            TimeRemaining = lockEntity.TimeRemaining,
            SessionId = lockEntity.SessionID,
            DeviceInfo = lockEntity.DeviceInfo
        };
    }
}
