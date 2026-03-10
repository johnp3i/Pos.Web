using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Represents a historical password hash for password reuse prevention.
/// Stores the last N password hashes to prevent users from reusing recent passwords.
/// </summary>
public class PasswordHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to AspNetUsers
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The hashed password (PBKDF2 hash from ASP.NET Core Identity)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the password was set
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username or ID of the person who changed the password (user themselves or admin)
    /// </summary>
    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    /// <summary>
    /// Reason for password change (e.g., "User requested", "Admin reset", "Security policy")
    /// </summary>
    [MaxLength(200)]
    public string? ChangeReason { get; set; }

    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
