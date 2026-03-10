using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Order lock repository interface for web.OrderLocks table
/// Following JDS repository design guidelines
/// </summary>
public interface IOrderLockRepository : IRepository<OrderLock>
{
    /// <summary>
    /// Get active lock for a specific order
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Active lock if exists, null otherwise</returns>
    Task<OrderLock?> GetActiveLockByOrderIdAsync(int orderId);

    /// <summary>
    /// Get all active locks for a specific user
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>List of active locks</returns>
    Task<IEnumerable<OrderLock>> GetActiveLocksByUserIdAsync(int userId);

    /// <summary>
    /// Get all expired locks that need cleanup
    /// </summary>
    /// <returns>List of expired locks</returns>
    Task<IEnumerable<OrderLock>> GetExpiredLocksAsync();

    /// <summary>
    /// Release lock for a specific order
    /// </summary>
    /// <param name="orderId">Order ID to release</param>
    /// <param name="userId">User ID releasing the lock</param>
    /// <returns>True if lock was released, false otherwise</returns>
    Task<bool> ReleaseLockAsync(int orderId, int userId);

    /// <summary>
    /// Extend lock expiration time
    /// </summary>
    /// <param name="orderId">Order ID to extend</param>
    /// <param name="userId">User ID extending the lock</param>
    /// <param name="additionalMinutes">Additional minutes to extend</param>
    /// <returns>True if lock was extended, false otherwise</returns>
    Task<bool> ExtendLockAsync(int orderId, int userId, int additionalMinutes);

    /// <summary>
    /// Cleanup expired locks (set IsActive to false)
    /// </summary>
    /// <returns>Number of locks cleaned up</returns>
    Task<int> CleanupExpiredLocksAsync();
}
