using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Feature flag repository interface for web.FeatureFlags table
/// Following JDS repository design guidelines
/// </summary>
public interface IFeatureFlagRepository : IRepository<FeatureFlag>
{
    /// <summary>
    /// Get feature flag by name
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <returns>Feature flag if found, null otherwise</returns>
    Task<FeatureFlag?> GetByNameAsync(string name);

    /// <summary>
    /// Get all enabled feature flags
    /// </summary>
    /// <returns>List of enabled feature flags</returns>
    Task<IEnumerable<FeatureFlag>> GetEnabledFlagsAsync();

    /// <summary>
    /// Check if a feature is enabled globally
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <returns>True if enabled, false otherwise</returns>
    Task<bool> IsFeatureEnabledAsync(string name);

    /// <summary>
    /// Check if a feature is enabled for a specific user
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if enabled for the user, false otherwise</returns>
    Task<bool> IsFeatureEnabledForUserAsync(string name, int userId);

    /// <summary>
    /// Check if a feature is enabled for a specific role
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="role">Role name to check</param>
    /// <returns>True if enabled for the role, false otherwise</returns>
    Task<bool> IsFeatureEnabledForRoleAsync(string name, string role);

    /// <summary>
    /// Enable a feature flag
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="updatedBy">User ID who is enabling the flag</param>
    /// <returns>True if enabled, false if not found</returns>
    Task<bool> EnableFeatureAsync(string name, int updatedBy);

    /// <summary>
    /// Disable a feature flag
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="updatedBy">User ID who is disabling the flag</param>
    /// <returns>True if disabled, false if not found</returns>
    Task<bool> DisableFeatureAsync(string name, int updatedBy);

    /// <summary>
    /// Update feature flag user restrictions
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="userIds">Array of user IDs to enable for (null for all users)</param>
    /// <param name="updatedBy">User ID who is updating the flag</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateUserRestrictionsAsync(string name, int[]? userIds, int updatedBy);

    /// <summary>
    /// Update feature flag role restrictions
    /// </summary>
    /// <param name="name">Feature flag name</param>
    /// <param name="roles">Array of role names to enable for (null for all roles)</param>
    /// <param name="updatedBy">User ID who is updating the flag</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateRoleRestrictionsAsync(string name, string[]? roles, int updatedBy);
}
