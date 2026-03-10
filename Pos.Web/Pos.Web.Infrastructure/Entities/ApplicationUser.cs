using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Extended ASP.NET Core Identity user with custom properties for POS system integration.
/// Links to legacy dbo.Users table via EmployeeId for backward compatibility.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Foreign key to legacy dbo.Users.ID table in POS database.
    /// Required for backward compatibility with legacy WPF POS system.
    /// </summary>
    [Required]
    public int EmployeeId { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    [MaxLength(50)]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [MaxLength(50)]
    public string? LastName { get; set; }

    /// <summary>
    /// Display name shown in UI (typically FirstName + LastName or legacy FullName)
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Indicates if the user account is active and can login
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last successful login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Timestamp when the password was last changed
    /// </summary>
    public DateTime? LastPasswordChangedAt { get; set; }

    /// <summary>
    /// Flag indicating if user must change password on next login
    /// (typically set to true for migrated users with temporary passwords)
    /// </summary>
    public bool RequirePasswordChange { get; set; } = false;

    /// <summary>
    /// Flag indicating if two-factor authentication is enabled for this user
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; } = false;

    // Navigation properties

    /// <summary>
    /// Collection of refresh tokens issued to this user
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Collection of active and historical sessions for this user
    /// </summary>
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    /// <summary>
    /// Collection of audit log entries for this user's authentication events
    /// </summary>
    public virtual ICollection<AuthAuditLog> AuditLogs { get; set; } = new List<AuthAuditLog>();

    /// <summary>
    /// Collection of historical password hashes for password reuse prevention
    /// </summary>
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
}
