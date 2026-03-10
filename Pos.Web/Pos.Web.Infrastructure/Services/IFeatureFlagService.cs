namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Feature flag service interface for dynamic feature toggling
/// Provides database-backed feature flags with in-memory caching
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Check if a feature is enabled globally
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <returns>True if feature is enabled globally</returns>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    /// Check if a feature is enabled for a specific user
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if feature is enabled for the user</returns>
    Task<bool> IsEnabledForUserAsync(string featureName, int userId);

    /// <summary>
    /// Check if a feature is enabled for a specific role
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <param name="role">Role name to check</param>
    /// <returns>True if feature is enabled for the role</returns>
    Task<bool> IsEnabledForRoleAsync(string featureName, string role);

    /// <summary>
    /// Check if a feature is enabled for a user with specific roles
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <param name="userId">User ID to check</param>
    /// <param name="roles">User's roles</param>
    /// <returns>True if feature is enabled for the user or any of their roles</returns>
    Task<bool> IsEnabledForUserOrRoleAsync(string featureName, int userId, IEnumerable<string> roles);

    /// <summary>
    /// Get all feature flags
    /// </summary>
    /// <returns>List of all feature flags</returns>
    Task<List<Entities.FeatureFlag>> GetAllFeaturesAsync();

    /// <summary>
    /// Get a specific feature flag by name
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <returns>Feature flag or null if not found</returns>
    Task<Entities.FeatureFlag?> GetFeatureAsync(string featureName);

    /// <summary>
    /// Create or update a feature flag
    /// </summary>
    /// <param name="featureName">Name of the feature</param>
    /// <param name="isEnabled">Whether the feature is enabled</param>
    /// <param name="description">Optional description</param>
    /// <param name="enabledForUserIds">Optional list of user IDs</param>
    /// <param name="enabledForRoles">Optional list of role names</param>
    /// <param name="updatedBy">User ID who updated the flag</param>
    Task SetFeatureAsync(
        string featureName,
        bool isEnabled,
        string? description = null,
        int[]? enabledForUserIds = null,
        string[]? enabledForRoles = null,
        int? updatedBy = null);

    /// <summary>
    /// Delete a feature flag
    /// </summary>
    /// <param name="featureName">Name of the feature to delete</param>
    Task DeleteFeatureAsync(string featureName);

    /// <summary>
    /// Refresh the feature flag cache
    /// Call this after updating feature flags in the database
    /// </summary>
    Task RefreshCacheAsync();
}
