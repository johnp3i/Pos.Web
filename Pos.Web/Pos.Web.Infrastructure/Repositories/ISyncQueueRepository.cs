using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Sync queue repository interface for web.SyncQueue table
/// Following JDS repository design guidelines
/// </summary>
public interface ISyncQueueRepository : IRepository<SyncQueue>
{
    /// <summary>
    /// Get pending sync operations for a specific device
    /// </summary>
    /// <param name="deviceId">Device ID to filter by</param>
    /// <returns>List of pending operations ordered by client timestamp</returns>
    Task<IEnumerable<SyncQueue>> GetPendingByDeviceIdAsync(string deviceId);

    /// <summary>
    /// Get pending sync operations for a specific user
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <returns>List of pending operations ordered by client timestamp</returns>
    Task<IEnumerable<SyncQueue>> GetPendingByUserIdAsync(int userId);

    /// <summary>
    /// Get all pending sync operations
    /// </summary>
    /// <param name="limit">Maximum number of operations to return</param>
    /// <returns>List of pending operations ordered by server timestamp</returns>
    Task<IEnumerable<SyncQueue>> GetPendingOperationsAsync(int limit = 100);

    /// <summary>
    /// Get failed sync operations that need retry
    /// </summary>
    /// <param name="maxAttempts">Maximum number of attempts before giving up</param>
    /// <returns>List of failed operations that can be retried</returns>
    Task<IEnumerable<SyncQueue>> GetFailedOperationsAsync(int maxAttempts = 3);

    /// <summary>
    /// Get sync operations by entity
    /// </summary>
    /// <param name="entityType">Entity type (e.g., "Order", "Customer")</param>
    /// <param name="entityId">Entity ID</param>
    /// <returns>List of sync operations for the entity</returns>
    Task<IEnumerable<SyncQueue>> GetByEntityAsync(string entityType, int entityId);

    /// <summary>
    /// Mark operation as processing
    /// </summary>
    /// <param name="id">Sync queue ID</param>
    /// <returns>True if marked, false if not found</returns>
    Task<bool> MarkAsProcessingAsync(long id);

    /// <summary>
    /// Mark operation as completed
    /// </summary>
    /// <param name="id">Sync queue ID</param>
    /// <returns>True if marked, false if not found</returns>
    Task<bool> MarkAsCompletedAsync(long id);

    /// <summary>
    /// Mark operation as failed
    /// </summary>
    /// <param name="id">Sync queue ID</param>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <returns>True if marked, false if not found</returns>
    Task<bool> MarkAsFailedAsync(long id, string errorMessage);

    /// <summary>
    /// Retry failed operation
    /// </summary>
    /// <param name="id">Sync queue ID</param>
    /// <returns>True if retried, false if not found or max attempts reached</returns>
    Task<bool> RetryOperationAsync(long id);

    /// <summary>
    /// Delete completed operations older than specified days
    /// </summary>
    /// <param name="daysToKeep">Number of days to keep</param>
    /// <returns>Number of operations deleted</returns>
    Task<int> DeleteCompletedOperationsAsync(int daysToKeep);

    /// <summary>
    /// Get sync queue statistics
    /// </summary>
    /// <returns>Dictionary with statistics (pending, processing, completed, failed counts)</returns>
    Task<Dictionary<string, int>> GetStatisticsAsync();
}
