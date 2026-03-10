using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Audit log repository interface for web.ApiAuditLog table
/// Following JDS repository design guidelines
/// </summary>
public interface IAuditLogRepository : IRepository<ApiAuditLog>
{
    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Paginated list of audit logs</returns>
    Task<IEnumerable<ApiAuditLog>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    /// <param name="entityType">Entity type (e.g., "Order", "Customer")</param>
    /// <param name="entityId">Entity ID</param>
    /// <returns>List of audit logs for the entity</returns>
    Task<IEnumerable<ApiAuditLog>> GetByEntityAsync(string entityType, int entityId);

    /// <summary>
    /// Get audit logs within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Paginated list of audit logs</returns>
    Task<IEnumerable<ApiAuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Get error logs (4xx and 5xx status codes)
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Paginated list of error logs</returns>
    Task<IEnumerable<ApiAuditLog>> GetErrorLogsAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Get audit logs by action type
    /// </summary>
    /// <param name="action">Action type (e.g., "OrderCreated", "PaymentProcessed")</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Paginated list of audit logs</returns>
    Task<IEnumerable<ApiAuditLog>> GetByActionAsync(string action, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Delete old audit logs (older than specified days)
    /// </summary>
    /// <param name="daysToKeep">Number of days to keep</param>
    /// <returns>Number of logs deleted</returns>
    Task<int> DeleteOldLogsAsync(int daysToKeep);

    /// <summary>
    /// Get audit log statistics for a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Dictionary with statistics (total requests, errors, avg duration, etc.)</returns>
    Task<Dictionary<string, object>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
}
