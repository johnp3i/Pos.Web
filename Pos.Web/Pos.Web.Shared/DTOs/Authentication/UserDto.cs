namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// User information DTO returned in authentication responses
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID (ApplicationUser.Id - GUID string)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Display name (shown in UI)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Employee ID (links to legacy dbo.Users.ID)
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// User roles (Admin, Manager, Cashier, Waiter, Kitchen)
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates if the user must change password on next login
    /// </summary>
    public bool RequirePasswordChange { get; set; }
}
