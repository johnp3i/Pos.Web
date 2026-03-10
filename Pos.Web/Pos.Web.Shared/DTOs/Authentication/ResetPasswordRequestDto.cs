using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// Request model for resetting user password (admin only)
/// </summary>
public class ResetPasswordRequestDto
{
    /// <summary>
    /// New password to set
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of new password
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
