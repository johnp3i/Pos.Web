using Pos.Web.Shared.DTOs.Migration;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service for migrating users from legacy dbo.Users table to WebPosMembership database
/// </summary>
public interface IUserMigrationService
{
    /// <summary>
    /// Migrates all active users from legacy dbo.Users table
    /// </summary>
    /// <param name="forcePasswordReset">If true, all migrated users will be required to change password on first login</param>
    /// <returns>Migration result with success/failure counts and error details</returns>
    Task<MigrationResult> MigrateAllUsersAsync(bool forcePasswordReset = true);

    /// <summary>
    /// Migrates a single user from legacy dbo.Users table
    /// </summary>
    /// <param name="legacyUserId">ID of the user in dbo.Users table</param>
    /// <param name="temporaryPassword">Temporary password for the migrated user (optional, will be generated if not provided)</param>
    /// <returns>Migration result for the single user</returns>
    Task<MigrationResult> MigrateSingleUserAsync(int legacyUserId, string? temporaryPassword = null);

    /// <summary>
    /// Gets the current migration status and statistics
    /// </summary>
    /// <returns>Migration report with statistics</returns>
    Task<MigrationReport> GetMigrationStatusAsync();

    /// <summary>
    /// Syncs user data from legacy system to WebPosMembership database
    /// Updates DisplayName and other fields from dbo.Users
    /// </summary>
    /// <param name="identityUserId">ID of the user in AspNetUsers table</param>
    /// <returns>True if sync was successful, false otherwise</returns>
    Task<bool> SyncUserDataAsync(string identityUserId);
}
