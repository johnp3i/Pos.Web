using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Represents an audit log entry for authentication and security events.
/// Provides comprehensive tracking for compliance, security monitoring, and troubleshooting.
/// </summary>
public class AuthAuditLog
{
    /// <summary>
    /// Primary key (using BIGINT for high-volume logging)
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Foreign key to AspNetUsers (nullable for failed login attempts with non-existent users)
    /// </summary>
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>
    /// Username attempted (stored even if user doesn't exist for security monitoring)
    /// </summary>
    [MaxLength(100)]
    public string? UserName { get; set; }

    /// <summary>
    /// Type of authentication event (LoginSuccess, LoginFailed, Logout, PasswordChanged, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address from which the event originated
    /// </summary>
    [MaxLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the browser/client
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional details about the event (JSON or plain text)
    /// </summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>
    /// Indicates if the event was successful (true) or failed (false)
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if the event failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    // Navigation property (nullable because user might not exist for failed logins)
    public virtual ApplicationUser? User { get; set; }
}
