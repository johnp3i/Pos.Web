using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Represents a JWT refresh token for token rotation and session management.
/// Refresh tokens have a longer lifespan (7 days) than access tokens (60 minutes).
/// </summary>
public class RefreshToken
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
    /// The refresh token value (cryptographically secure random string)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the token expires (typically 7 days from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when the token was revoked (null if still active)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for token revocation (e.g., "User logout", "Password changed", "Security event")
    /// </summary>
    [MaxLength(200)]
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Device information (e.g., "Chrome on Windows", "Safari on iPhone")
    /// </summary>
    [MaxLength(200)]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address from which the token was issued
    /// </summary>
    [MaxLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }

    // Navigation property
    public virtual ApplicationUser? User { get; set; }

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
