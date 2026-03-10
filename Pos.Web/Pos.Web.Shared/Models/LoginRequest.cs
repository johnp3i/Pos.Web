using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
