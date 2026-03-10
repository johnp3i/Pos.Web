using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using System.Text.Json;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Feature flag repository for web.FeatureFlags table
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class FeatureFlagRepository : GenericRepository<FeatureFlag>, IFeatureFlagRepository
{
    public FeatureFlagRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get feature flag by name
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<FeatureFlag?> GetByNameAsync(string name)
    {
        try
        {
            return await _dbSet
                .Include(x => x.UpdatedByUser)
                .FirstOrDefaultAsync(x => x.Name == name);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all enabled feature flags
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<FeatureFlag>> GetEnabledFlagsAsync()
    {
        try
        {
            return await _dbSet
                .Include(x => x.UpdatedByUser)
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Check if a feature is enabled globally
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> IsFeatureEnabledAsync(string name)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
            return flag?.IsEnabled ?? false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Check if a feature is enabled for a specific user
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> IsFeatureEnabledForUserAsync(string name, int userId)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
            return flag?.IsEnabledForUser(userId) ?? false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Check if a feature is enabled for a specific role
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> IsFeatureEnabledForRoleAsync(string name, string role)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
            return flag?.IsEnabledForRole(role) ?? false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Enable a feature flag
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> EnableFeatureAsync(string name, int updatedBy)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);

            if (flag == null)
            {
                return false;
            }

            flag.IsEnabled = true;
            flag.UpdatedAt = DateTime.UtcNow;
            flag.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Disable a feature flag
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> DisableFeatureAsync(string name, int updatedBy)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);

            if (flag == null)
            {
                return false;
            }

            flag.IsEnabled = false;
            flag.UpdatedAt = DateTime.UtcNow;
            flag.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Update feature flag user restrictions
    /// Following JDS guideline: async/await with try/catch, null-safe parameters
    /// </summary>
    public async Task<bool> UpdateUserRestrictionsAsync(string name, int[]? userIds, int updatedBy)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);

            if (flag == null)
            {
                return false;
            }

            flag.EnabledForUserIDs = userIds != null && userIds.Length > 0
                ? JsonSerializer.Serialize(userIds)
                : null;
            flag.UpdatedAt = DateTime.UtcNow;
            flag.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Update feature flag role restrictions
    /// Following JDS guideline: async/await with try/catch, null-safe parameters
    /// </summary>
    public async Task<bool> UpdateRoleRestrictionsAsync(string name, string[]? roles, int updatedBy)
    {
        try
        {
            var flag = await _dbSet.FirstOrDefaultAsync(x => x.Name == name);

            if (flag == null)
            {
                return false;
            }

            flag.EnabledForRoles = roles != null && roles.Length > 0
                ? JsonSerializer.Serialize(roles)
                : null;
            flag.UpdatedAt = DateTime.UtcNow;
            flag.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
