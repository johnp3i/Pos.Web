namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// Request DTO for first-login password change (without requiring authentication token).
/// Used when migrated users need to change their temporary password before they can login.
/// </summary>
public class FirstLoginPasswordChangeRequestDto
{
    /// <summary>
    /// Username of the user changing their password
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Current (temporary) password
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password that meets complexity requirements
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Device type for session tracking (optional, defaults to "Desktop")
    /// </summary>
    public string? DeviceType { get; set; }
}
