using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Feature flag entity to enable/disable features dynamically
/// Mapped to web.FeatureFlags table
/// </summary>
[Table("FeatureFlags", Schema = "web")]
public class FeatureFlag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// JSON array of user IDs for which this feature is enabled
    /// Example: "[1, 5, 10]"
    /// </summary>
    public string? EnabledForUserIDs { get; set; }

    /// <summary>
    /// JSON array of role names for which this feature is enabled
    /// Example: "["Admin", "Manager"]"
    /// </summary>
    public string? EnabledForRoles { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UpdatedBy))]
    public User? UpdatedByUser { get; set; }

    /// <summary>
    /// Checks if the feature is enabled for a specific user ID
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if feature is enabled for the user</returns>
    public bool IsEnabledForUser(int userId)
    {
        if (!IsEnabled) return false;
        if (string.IsNullOrEmpty(EnabledForUserIDs)) return true;

        try
        {
            var userIds = System.Text.Json.JsonSerializer.Deserialize<int[]>(EnabledForUserIDs);
            return userIds?.Contains(userId) ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the feature is enabled for a specific role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if feature is enabled for the role</returns>
    public bool IsEnabledForRole(string role)
    {
        if (!IsEnabled) return false;
        if (string.IsNullOrEmpty(EnabledForRoles)) return true;

        try
        {
            var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(EnabledForRoles);
            return roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }
}
