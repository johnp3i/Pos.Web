namespace Pos.Web.Shared.DTOs.Migration;

/// <summary>
/// Result of a user migration operation
/// </summary>
public class MigrationResult
{
    /// <summary>
    /// Total number of users processed
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Number of users successfully migrated
    /// </summary>
    public int SuccessfulMigrations { get; set; }

    /// <summary>
    /// Number of users that failed to migrate
    /// </summary>
    public int FailedMigrations { get; set; }

    /// <summary>
    /// Number of users skipped (already migrated)
    /// </summary>
    public int SkippedUsers { get; set; }

    /// <summary>
    /// List of errors encountered during migration
    /// </summary>
    public List<MigrationError> Errors { get; set; } = new();

    /// <summary>
    /// List of successfully migrated users with their temporary passwords
    /// </summary>
    public List<MigratedUserInfo> MigratedUsers { get; set; } = new();

    /// <summary>
    /// Duration of the migration operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Indicates if the migration was successful overall
    /// </summary>
    public bool IsSuccessful => FailedMigrations == 0;

    /// <summary>
    /// Summary message of the migration operation
    /// </summary>
    public string Summary => $"Migration completed in {Duration.TotalSeconds:F2}s. " +
                            $"Total: {TotalUsers}, Successful: {SuccessfulMigrations}, " +
                            $"Failed: {FailedMigrations}, Skipped: {SkippedUsers}";
}
