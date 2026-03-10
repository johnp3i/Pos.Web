using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Feature flag service implementation with database-backed flags and in-memory caching
/// Provides dynamic feature toggling with user-specific and role-specific evaluation
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly PosDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagService> _logger;
    private const string CacheKeyPrefix = "feature_flag:";
    private const string AllFeaturesCacheKey = "feature_flags:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public FeatureFlagService(
        PosDbContext context,
        IMemoryCache cache,
        ILogger<FeatureFlagService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if a feature is enabled globally
    /// </summary>
    public async Task<bool> IsEnabledAsync(string featureName)
    {
        try
        {
            var feature = await GetFeatureFromCacheOrDbAsync(featureName);
            return feature?.IsEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature is enabled: {FeatureName}", featureName);
            return false;
        }
    }

    /// <summary>
    /// Check if a feature is enabled for a specific user
    /// </summary>
    public async Task<bool> IsEnabledForUserAsync(string featureName, int userId)
    {
        try
        {
            var feature = await GetFeatureFromCacheOrDbAsync(featureName);
            return feature?.IsEnabledForUser(userId) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature is enabled for user: {FeatureName}, UserId: {UserId}", featureName, userId);
            return false;
        }
    }

    /// <summary>
    /// Check if a feature is enabled for a specific role
    /// </summary>
    public async Task<bool> IsEnabledForRoleAsync(string featureName, string role)
    {
        try
        {
            var feature = await GetFeatureFromCacheOrDbAsync(featureName);
            return feature?.IsEnabledForRole(role) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature is enabled for role: {FeatureName}, Role: {Role}", featureName, role);
            return false;
        }
    }

    /// <summary>
    /// Check if a feature is enabled for a user with specific roles
    /// </summary>
    public async Task<bool> IsEnabledForUserOrRoleAsync(string featureName, int userId, IEnumerable<string> roles)
    {
        try
        {
            var feature = await GetFeatureFromCacheOrDbAsync(featureName);
            if (feature == null || !feature.IsEnabled)
            {
                return false;
            }

            // Check if enabled for user
            if (feature.IsEnabledForUser(userId))
            {
                return true;
            }

            // Check if enabled for any of the user's roles
            foreach (var role in roles)
            {
                if (feature.IsEnabledForRole(role))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature is enabled for user or role: {FeatureName}, UserId: {UserId}", featureName, userId);
            return false;
        }
    }

    /// <summary>
    /// Get all feature flags
    /// </summary>
    public async Task<List<FeatureFlag>> GetAllFeaturesAsync()
    {
        try
        {
            return await _cache.GetOrCreateAsync(AllFeaturesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
                
                var features = await _context.FeatureFlags
                    .AsNoTracking()
                    .OrderBy(f => f.Name)
                    .ToListAsync();

                _logger.LogDebug("Loaded {Count} feature flags from database", features.Count);
                return features;
            }) ?? new List<FeatureFlag>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all feature flags");
            return new List<FeatureFlag>();
        }
    }

    /// <summary>
    /// Get a specific feature flag by name
    /// </summary>
    public async Task<FeatureFlag?> GetFeatureAsync(string featureName)
    {
        try
        {
            return await GetFeatureFromCacheOrDbAsync(featureName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flag: {FeatureName}", featureName);
            return null;
        }
    }

    /// <summary>
    /// Create or update a feature flag
    /// </summary>
    public async Task SetFeatureAsync(
        string featureName,
        bool isEnabled,
        string? description = null,
        int[]? enabledForUserIds = null,
        string[]? enabledForRoles = null,
        int? updatedBy = null)
    {
        try
        {
            var feature = await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Name == featureName);

            if (feature == null)
            {
                // Create new feature flag
                feature = new FeatureFlag
                {
                    Name = featureName,
                    Description = description,
                    IsEnabled = isEnabled,
                    EnabledForUserIDs = enabledForUserIds != null ? JsonSerializer.Serialize(enabledForUserIds) : null,
                    EnabledForRoles = enabledForRoles != null ? JsonSerializer.Serialize(enabledForRoles) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = updatedBy
                };

                _context.FeatureFlags.Add(feature);
                _logger.LogInformation("Created new feature flag: {FeatureName}, Enabled: {IsEnabled}", featureName, isEnabled);
            }
            else
            {
                // Update existing feature flag
                feature.Description = description ?? feature.Description;
                feature.IsEnabled = isEnabled;
                feature.EnabledForUserIDs = enabledForUserIds != null ? JsonSerializer.Serialize(enabledForUserIds) : feature.EnabledForUserIDs;
                feature.EnabledForRoles = enabledForRoles != null ? JsonSerializer.Serialize(enabledForRoles) : feature.EnabledForRoles;
                feature.UpdatedAt = DateTime.UtcNow;
                feature.UpdatedBy = updatedBy;

                _logger.LogInformation("Updated feature flag: {FeatureName}, Enabled: {IsEnabled}", featureName, isEnabled);
            }

            await _context.SaveChangesAsync();

            // Invalidate cache
            InvalidateCache(featureName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature flag: {FeatureName}", featureName);
            throw;
        }
    }

    /// <summary>
    /// Delete a feature flag
    /// </summary>
    public async Task DeleteFeatureAsync(string featureName)
    {
        try
        {
            var feature = await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Name == featureName);

            if (feature != null)
            {
                _context.FeatureFlags.Remove(feature);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted feature flag: {FeatureName}", featureName);

                // Invalidate cache
                InvalidateCache(featureName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature flag: {FeatureName}", featureName);
            throw;
        }
    }

    /// <summary>
    /// Refresh the feature flag cache
    /// </summary>
    public async Task RefreshCacheAsync()
    {
        try
        {
            _cache.Remove(AllFeaturesCacheKey);
            
            // Reload all features into cache
            await GetAllFeaturesAsync();
            
            _logger.LogInformation("Feature flag cache refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing feature flag cache");
            throw;
        }
    }

    /// <summary>
    /// Get feature from cache or database
    /// </summary>
    private async Task<FeatureFlag?> GetFeatureFromCacheOrDbAsync(string featureName)
    {
        var cacheKey = $"{CacheKeyPrefix}{featureName}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            var feature = await _context.FeatureFlags
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Name == featureName);

            if (feature == null)
            {
                _logger.LogDebug("Feature flag not found: {FeatureName}", featureName);
            }

            return feature;
        });
    }

    /// <summary>
    /// Invalidate cache for a specific feature
    /// </summary>
    private void InvalidateCache(string featureName)
    {
        var cacheKey = $"{CacheKeyPrefix}{featureName}";
        _cache.Remove(cacheKey);
        _cache.Remove(AllFeaturesCacheKey);
        _logger.LogDebug("Invalidated cache for feature: {FeatureName}", featureName);
    }
}
