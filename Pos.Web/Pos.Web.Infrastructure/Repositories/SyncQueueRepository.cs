using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Sync queue repository for web.SyncQueue table
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class SyncQueueRepository : GenericRepository<SyncQueue>, ISyncQueueRepository
{
    public SyncQueueRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get pending sync operations for a specific device
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<SyncQueue>> GetPendingByDeviceIdAsync(string deviceId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.DeviceID == deviceId && x.Status == "Pending")
                .OrderBy(x => x.ClientTimestamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get pending sync operations for a specific user
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<SyncQueue>> GetPendingByUserIdAsync(int userId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.UserID == userId && x.Status == "Pending")
                .OrderBy(x => x.ClientTimestamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all pending sync operations
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<SyncQueue>> GetPendingOperationsAsync(int limit = 100)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.Status == "Pending")
                .OrderBy(x => x.ServerTimestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get failed sync operations that need retry
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<SyncQueue>> GetFailedOperationsAsync(int maxAttempts = 3)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.Status == "Failed" && x.AttemptCount < maxAttempts)
                .OrderBy(x => x.ServerTimestamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get sync operations by entity
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<SyncQueue>> GetByEntityAsync(string entityType, int entityId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.EntityType == entityType && x.EntityID == entityId)
                .OrderBy(x => x.ClientTimestamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Mark operation as processing
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> MarkAsProcessingAsync(long id)
    {
        try
        {
            var operation = await _dbSet.FirstOrDefaultAsync(x => x.ID == id);

            if (operation == null)
            {
                return false;
            }

            operation.Status = "Processing";
            operation.LastAttemptAt = DateTime.UtcNow;
            operation.AttemptCount++;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Mark operation as completed
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> MarkAsCompletedAsync(long id)
    {
        try
        {
            var operation = await _dbSet.FirstOrDefaultAsync(x => x.ID == id);

            if (operation == null)
            {
                return false;
            }

            operation.Status = "Completed";
            operation.ProcessedAt = DateTime.UtcNow;
            operation.ErrorMessage = null;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Mark operation as failed
    /// Following JDS guideline: async/await with try/catch, null-safe parameters
    /// </summary>
    public async Task<bool> MarkAsFailedAsync(long id, string errorMessage)
    {
        try
        {
            var operation = await _dbSet.FirstOrDefaultAsync(x => x.ID == id);

            if (operation == null)
            {
                return false;
            }

            operation.Status = "Failed";
            operation.ErrorMessage = errorMessage ?? "Unknown error";
            operation.LastAttemptAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retry failed operation
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> RetryOperationAsync(long id)
    {
        try
        {
            var operation = await _dbSet.FirstOrDefaultAsync(x => x.ID == id);

            if (operation == null || operation.Status != "Failed")
            {
                return false;
            }

            // Check if max attempts reached (default 3)
            if (operation.AttemptCount >= 3)
            {
                return false;
            }

            operation.Status = "Pending";
            operation.ErrorMessage = null;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete completed operations older than specified days
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<int> DeleteCompletedOperationsAsync(int daysToKeep)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldOperations = await _dbSet
                .Where(x => x.Status == "Completed" && x.ProcessedAt.HasValue && x.ProcessedAt.Value < cutoffDate)
                .ToListAsync();

            if (!oldOperations.Any())
            {
                return 0;
            }

            _dbSet.RemoveRange(oldOperations);
            await _context.SaveChangesAsync();
            return oldOperations.Count;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get sync queue statistics
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<Dictionary<string, int>> GetStatisticsAsync()
    {
        try
        {
            var statistics = new Dictionary<string, int>
            {
                ["Pending"] = await _dbSet.CountAsync(x => x.Status == "Pending"),
                ["Processing"] = await _dbSet.CountAsync(x => x.Status == "Processing"),
                ["Completed"] = await _dbSet.CountAsync(x => x.Status == "Completed"),
                ["Failed"] = await _dbSet.CountAsync(x => x.Status == "Failed"),
                ["Total"] = await _dbSet.CountAsync()
            };

            return statistics;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
