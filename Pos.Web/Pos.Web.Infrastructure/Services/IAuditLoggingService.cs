using Pos.Web.Shared.DTOs.Audit;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for audit logging operations.
/// Records authentication and authorization events for security monitoring and compliance.
/// </summary>
public interface IAuditLoggingService
{
    /// <summary>
    /// Logs a login attempt (success or failure)
    /// </summary>
    /// <param name="username">Username attempting to login</param>
    /// <param name="success">Whether the login was successful</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    /// <param name="errorMessage">Error message if login failed</param>
    Task LogLoginAttemptAsync(
        string username, 
        bool success, 
        string? ipAddress, 
        string? userAgent,
        string? errorMessage = null);

    /// <summary>
    /// Logs a logout event
    /// </summary>
    /// <param name="userId">User ID who logged out</param>
    /// <param name="sessionId">Session ID that was ended</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    Task LogLogoutAsync(
        string userId, 
        Guid sessionId,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Logs a password change event
    /// </summary>
    /// <param name="userId">User ID whose password was changed</param>
    /// <param name="success">Whether the password change was successful</param>
    /// <param name="changedBy">User ID who initiated the change (for admin resets)</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    Task LogPasswordChangeAsync(
        string userId, 
        bool success,
        string? changedBy = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Logs an account lockout event
    /// </summary>
    /// <param name="userId">User ID whose account was locked</param>
    /// <param name="reason">Reason for lockout</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    Task LogAccountLockoutAsync(
        string userId, 
        string reason,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Logs an account unlock event
    /// </summary>
    /// <param name="userId">User ID whose account was unlocked</param>
    /// <param name="unlockedBy">User ID who unlocked the account</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    Task LogAccountUnlockAsync(
        string userId,
        string unlockedBy,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Logs a token refresh event
    /// </summary>
    /// <param name="userId">User ID whose token was refreshed</param>
    /// <param name="success">Whether the token refresh was successful</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    /// <param name="errorMessage">Error message if refresh failed</param>
    Task LogTokenRefreshAsync(
        string userId, 
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null);

    /// <summary>
    /// Logs a generic security event
    /// </summary>
    /// <param name="eventType">Type of security event</param>
    /// <param name="userId">User ID associated with the event (if applicable)</param>
    /// <param name="details">Additional details about the event</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the client</param>
    Task LogSecurityEventAsync(
        AuditEventType eventType, 
        string? userId, 
        string details,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Retrieves audit logs for a specific user
    /// </summary>
    /// <param name="userId">User ID to query logs for</param>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <returns>List of audit log entries</returns>
    Task<List<AuthAuditLogDto>> GetUserAuditLogsAsync(
        string userId, 
        DateTime? from = null, 
        DateTime? to = null);

    /// <summary>
    /// Retrieves audit logs filtered by event type
    /// </summary>
    /// <param name="eventType">Event type to filter by</param>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of audit log entries</returns>
    Task<List<AuthAuditLogDto>> GetAuditLogsByEventTypeAsync(
        AuditEventType eventType,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>
    /// Retrieves failed login attempts
    /// </summary>
    /// <param name="from">Start date for the query (optional)</param>
    /// <param name="to">End date for the query (optional)</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of failed login audit log entries</returns>
    Task<List<AuthAuditLogDto>> GetFailedLoginAttemptsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);
}
