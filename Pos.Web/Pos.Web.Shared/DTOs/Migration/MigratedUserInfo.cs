namespace Pos.Web.Shared.DTOs.Migration;

/// <summary>
/// Information about a successfully migrated user
/// </summary>
public class MigratedUserInfo
{
    /// <summary>
    /// ID of the user in the legacy dbo.Users table
    /// </summary>
    public int LegacyUserId { get; set; }

    /// <summary>
    /// ID of the user in AspNetUsers table
    /// </summary>
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Assigned role
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Temporary password generated for the user
    /// WARNING: This should be securely distributed to users and not logged
    /// </summary>
    public string TemporaryPassword { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user was migrated
    /// </summary>
    public DateTime MigratedAt { get; set; } = DateTime.UtcNow;
}
