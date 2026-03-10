namespace Pos.Web.Shared.DTOs.Migration;

/// <summary>
/// Report of migration status and statistics
/// </summary>
public class MigrationReport
{
    /// <summary>
    /// Total number of users in legacy dbo.Users table (active only)
    /// </summary>
    public int TotalLegacyUsers { get; set; }

    /// <summary>
    /// Number of users already migrated to WebPosMembership
    /// </summary>
    public int MigratedUsersCount { get; set; }

    /// <summary>
    /// Number of users pending migration
    /// </summary>
    public int PendingMigrationCount { get; set; }

    /// <summary>
    /// Percentage of users migrated
    /// </summary>
    public double MigrationPercentage => TotalLegacyUsers > 0 
        ? (double)MigratedUsersCount / TotalLegacyUsers * 100 
        : 0;

    /// <summary>
    /// Indicates if all users have been migrated
    /// </summary>
    public bool IsComplete => PendingMigrationCount == 0;

    /// <summary>
    /// Last migration timestamp
    /// </summary>
    public DateTime? LastMigrationDate { get; set; }

    /// <summary>
    /// Summary message
    /// </summary>
    public string Summary => $"Migration Status: {MigrationPercentage:F1}% complete. " +
                            $"Migrated: {MigratedUsersCount}/{TotalLegacyUsers}, " +
                            $"Pending: {PendingMigrationCount}";
}
