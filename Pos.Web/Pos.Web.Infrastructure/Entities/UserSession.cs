using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// User session entity for tracking active user sessions in the membership database.
/// Mapped to dbo.UserSessions table in WebPosMembership database.
/// </summary>
[Table("UserSessions")]
public class UserSession
{
    /// <summary>
    /// Unique session identifier (Primary Key)
    /// </summary>
    [Key]
    public Guid SessionId { get; set; }

    /// <summary>
    /// Foreign key to AspNetUsers.Id (string)
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Device type (Desktop, Tablet, Mobile)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Additional device information
    /// </summary>
    [MaxLength(200)]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address of the client
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the browser
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Whether the session is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the session was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last activity timestamp
    /// </summary>
    [Required]
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// When the session was ended (null if still active)
    /// </summary>
    public DateTime? EndedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Navigation property to the user who owns this session
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    // Computed properties

    /// <summary>
    /// Checks if the session is still valid (active and not ended)
    /// </summary>
    [NotMapped]
    public bool IsValid => IsActive && !EndedAt.HasValue;

    /// <summary>
    /// Gets the session duration
    /// </summary>
    [NotMapped]
    public TimeSpan SessionDuration => EndedAt.HasValue 
        ? EndedAt.Value - CreatedAt 
        : DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Gets the time since last activity
    /// </summary>
    [NotMapped]
    public TimeSpan TimeSinceLastActivity => DateTime.UtcNow - LastActivityAt;
}
