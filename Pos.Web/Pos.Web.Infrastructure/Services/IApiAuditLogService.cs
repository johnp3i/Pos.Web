using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// API audit log service interface for tracking API operations
/// Records all API requests, responses, and entity changes for compliance and debugging
/// </summary>
public interface IApiAuditLogService
{
    /// <summary>
    /// Log an API request with response details
    /// </summary>
    /// <param name="userId">User ID making the request (if authenticated)</param>
    /// <param name="action">Action being performed (e.g., "CreateOrder", "UpdateCustomer")</param>
    /// <param name="requestPath">HTTP request path</param>
    /// <param name="requestMethod">HTTP request method (GET, POST, PUT, DELETE)</param>
    /// <param name="statusCode">HTTP response status code</param>
    /// <param name="duration">Request duration in milliseconds</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent</param>
    /// <param name="errorMessage">Error message if request failed</param>
    Task LogApiRequestAsync(
        int? userId,
        string action,
        string requestPath,
        string requestMethod,
        int statusCode,
        int duration,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null);

    /// <summary>
    /// Log an entity change (create, update, delete)
    /// </summary>
    /// <param name="userId">User ID making the change</param>
    /// <param name="action">Action being performed (Create, Update, Delete)</param>
    /// <param name="entityType">Type of entity being changed (e.g., "Order", "Customer")</param>
    /// <param name="entityId">ID of the entity being changed</param>
    /// <param name="oldValues">JSON representation of old values (for updates)</param>
    /// <param name="newValues">JSON representation of new values</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent</param>
    Task LogEntityChangeAsync(
        int? userId,
        string action,
        string entityType,
        int entityId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Log an entity change with automatic change tracking
    /// Compares old and new entity states and serializes differences
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="userId">User ID making the change</param>
    /// <param name="action">Action being performed (Create, Update, Delete)</param>
    /// <param name="oldEntity">Old entity state (null for creates)</param>
    /// <param name="newEntity">New entity state (null for deletes)</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent</param>
    Task LogEntityChangeWithTrackingAsync<T>(
        int? userId,
        string action,
        T? oldEntity,
        T? newEntity,
        string? ipAddress = null,
        string? userAgent = null) where T : class;

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <returns>List of audit log entries</returns>
    Task<List<ApiAuditLog>> GetEntityAuditLogsAsync(
        string entityType,
        int entityId,
        DateTime? from = null,
        DateTime? to = null);

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    /// <param name="userId">User ID to query logs for</param>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of audit log entries</returns>
    Task<List<ApiAuditLog>> GetUserAuditLogsAsync(
        int userId,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>
    /// Get failed API requests (4xx and 5xx status codes)
    /// </summary>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of failed request audit log entries</returns>
    Task<List<ApiAuditLog>> GetFailedRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>
    /// Get audit logs by action
    /// </summary>
    /// <param name="action">Action to filter by (e.g., "CreateOrder", "UpdateCustomer")</param>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of audit log entries</returns>
    Task<List<ApiAuditLog>> GetAuditLogsByActionAsync(
        string action,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);
}
