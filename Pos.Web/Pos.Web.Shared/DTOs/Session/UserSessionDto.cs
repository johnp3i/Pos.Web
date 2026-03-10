namespace Pos.Web.Shared.DTOs.Session;

/// <summary>
/// Data transfer object for user session information.
/// Contains session details without sensitive data.
/// </summary>
public class UserSessionDto
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Type of device (Desktop, Tablet, Mobile)
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Detailed device information
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address from which the session was initiated
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Timestamp when the session was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of the last activity in this session
    /// </summary>
    public DateTime? LastActivityAt { get; set; }
}
