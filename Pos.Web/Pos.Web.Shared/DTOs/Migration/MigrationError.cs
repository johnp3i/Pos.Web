namespace Pos.Web.Shared.DTOs.Migration;

/// <summary>
/// Error information for a failed user migration
/// </summary>
public class MigrationError
{
    /// <summary>
    /// ID of the user in the legacy dbo.Users table
    /// </summary>
    public int LegacyUserId { get; set; }

    /// <summary>
    /// Username from the legacy system
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Error message describing why the migration failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
