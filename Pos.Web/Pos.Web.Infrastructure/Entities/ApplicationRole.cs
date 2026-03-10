using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Extended ASP.NET Core Identity role with custom properties for POS system.
/// Defines the five system roles: Admin, Manager, Cashier, Waiter, Kitchen.
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Human-readable description of the role's purpose and permissions
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp when the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if this is a system-defined role that cannot be deleted
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    // System role constants
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string Waiter = "Waiter";
    public const string Kitchen = "Kitchen";

    /// <summary>
    /// Gets all system role names
    /// </summary>
    public static string[] GetSystemRoles() => new[]
    {
        Admin,
        Manager,
        Cashier,
        Waiter,
        Kitchen
    };

    /// <summary>
    /// Checks if a role name is a system role
    /// </summary>
    public static bool IsSystemRoleName(string roleName)
    {
        return roleName switch
        {
            Admin or Manager or Cashier or Waiter or Kitchen => true,
            _ => false
        };
    }
}
