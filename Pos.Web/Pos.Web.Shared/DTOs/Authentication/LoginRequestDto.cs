using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// Login request DTO for WebPosMembership authentication
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Username (required)
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 256 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password (required)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Device type (Desktop, Tablet, Mobile)
    /// </summary>
    public string DeviceType { get; set; } = "Desktop";

    /// <summary>
    /// Remember me flag (extends refresh token expiration)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
