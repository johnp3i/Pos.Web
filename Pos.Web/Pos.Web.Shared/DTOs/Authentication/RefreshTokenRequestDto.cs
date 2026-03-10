using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// The refresh token to use for obtaining a new access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
