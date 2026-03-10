namespace Pos.Web.Shared.DTOs.Audit;

/// <summary>
/// Data transfer object for authentication audit log entries
/// </summary>
public class AuthAuditLogDto
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// User ID associated with the event (null for failed logins with non-existent users)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username associated with the event
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Type of audit event (LoginSuccess, LoginFailed, Logout, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP address of the client
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional details about the event
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Whether the event was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if the event failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
