using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Order lock service interface for managing order locking to prevent concurrent edits
/// Implements optimistic locking with automatic expiration
/// </summary>
public interface IOrderLockService
{
    /// <summary>
    /// Acquire a lock on an order for editing
    /// </summary>
    /// <param name="orderId">Order ID to lock</param>
    /// <param name="userId">User ID acquiring the lock</param>
    /// <param name="sessionId">Session ID for tracking</param>
    /// <param name="deviceInfo">Device information for tracking</param>
    /// <param name="timeoutMinutes">Lock timeout in minutes (default 15)</param>
    /// <returns>Lock result with success status and lock details</returns>
    Task<OrderLockResultDto> AcquireLockAsync(
        int orderId, 
        int userId, 
        string? sessionId = null, 
        string? deviceInfo = null, 
        int timeoutMinutes = 15);

    /// <summary>
    /// Release a lock on an order
    /// </summary>
    /// <param name="orderId">Order ID to unlock</param>
    /// <param name="userId">User ID releasing the lock</param>
    /// <returns>True if lock was released, false if lock not found or not owned by user</returns>
    Task<bool> ReleaseLockAsync(int orderId, int userId);

    /// <summary>
    /// Get the current lock status for an order
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Lock status information</returns>
    Task<OrderLockStatusDto> GetLockStatusAsync(int orderId);

    /// <summary>
    /// Extend the expiration time of an existing lock
    /// </summary>
    /// <param name="orderId">Order ID to extend</param>
    /// <param name="userId">User ID extending the lock</param>
    /// <param name="additionalMinutes">Additional minutes to extend (default 15)</param>
    /// <returns>True if lock was extended, false if lock not found or expired</returns>
    Task<bool> ExtendLockAsync(int orderId, int userId, int additionalMinutes = 15);

    /// <summary>
    /// Force release a lock (admin operation)
    /// </summary>
    /// <param name="orderId">Order ID to unlock</param>
    /// <param name="adminUserId">Admin user ID performing the operation</param>
    /// <returns>True if lock was released, false if no lock exists</returns>
    Task<bool> ForceReleaseLockAsync(int orderId, int adminUserId);

    /// <summary>
    /// Get all active locks for a specific user
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>List of active locks</returns>
    Task<List<OrderLockStatusDto>> GetUserLocksAsync(int userId);

    /// <summary>
    /// Cleanup expired locks (background task)
    /// </summary>
    /// <returns>Number of locks cleaned up</returns>
    Task<int> CleanupExpiredLocksAsync();

    /// <summary>
    /// Check if an order is locked by another user
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <param name="userId">Current user ID</param>
    /// <returns>True if locked by another user, false otherwise</returns>
    Task<bool> IsLockedByAnotherUserAsync(int orderId, int userId);
}

/// <summary>
/// Result of a lock acquisition attempt
/// </summary>
public class OrderLockResultDto
{
    /// <summary>
    /// Whether the lock was successfully acquired
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Lock ID if successful
    /// </summary>
    public int? LockId { get; set; }

    /// <summary>
    /// Lock expiration time if successful
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Error message if unsuccessful
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Existing lock information if order is already locked
    /// </summary>
    public OrderLockStatusDto? ExistingLock { get; set; }
}

/// <summary>
/// Current lock status for an order
/// </summary>
public class OrderLockStatusDto
{
    /// <summary>
    /// Whether the order is currently locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Lock ID if locked
    /// </summary>
    public int? LockId { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// User ID who holds the lock
    /// </summary>
    public int? LockedByUserId { get; set; }

    /// <summary>
    /// User name who holds the lock
    /// </summary>
    public string? LockedByUserName { get; set; }

    /// <summary>
    /// When the lock was acquired
    /// </summary>
    public DateTime? LockAcquiredAt { get; set; }

    /// <summary>
    /// When the lock expires
    /// </summary>
    public DateTime? LockExpiresAt { get; set; }

    /// <summary>
    /// Time remaining before lock expires
    /// </summary>
    public TimeSpan? TimeRemaining { get; set; }

    /// <summary>
    /// Session ID of the lock holder
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Device information of the lock holder
    /// </summary>
    public string? DeviceInfo { get; set; }
}
