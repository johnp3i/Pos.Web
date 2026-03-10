using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Audit log repository for web.ApiAuditLog table
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class AuditLogRepository : GenericRepository<ApiAuditLog>, IAuditLogRepository
{
    public AuditLogRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<ApiAuditLog>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<ApiAuditLog>> GetByEntityAsync(string entityType, int entityId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.EntityType == entityType && x.EntityID == entityId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get audit logs within a date range
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<ApiAuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get error logs (4xx and 5xx status codes)
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<ApiAuditLog>> GetErrorLogsAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.StatusCode.HasValue && x.StatusCode >= 400)
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get audit logs by action type
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<ApiAuditLog>> GetByActionAsync(string action, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.Action == action)
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete old audit logs (older than specified days)
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<int> DeleteOldLogsAsync(int daysToKeep)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldLogs = await _dbSet
                .Where(x => x.Timestamp < cutoffDate)
                .ToListAsync();

            if (!oldLogs.Any())
            {
                return 0;
            }

            _dbSet.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
            return oldLogs.Count;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get audit log statistics for a date range
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<Dictionary<string, object>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var logs = await _dbSet
                .Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
                .ToListAsync();

            var statistics = new Dictionary<string, object>
            {
                ["TotalRequests"] = logs.Count,
                ["SuccessfulRequests"] = logs.Count(x => x.IsSuccessful),
                ["ErrorRequests"] = logs.Count(x => x.IsError),
                ["AverageDuration"] = logs.Where(x => x.Duration.HasValue).Average(x => x.Duration ?? 0),
                ["MaxDuration"] = logs.Where(x => x.Duration.HasValue).Max(x => x.Duration ?? 0),
                ["MinDuration"] = logs.Where(x => x.Duration.HasValue).Min(x => x.Duration ?? 0),
                ["UniqueUsers"] = logs.Where(x => x.UserID.HasValue).Select(x => x.UserID).Distinct().Count(),
                ["TopActions"] = logs.GroupBy(x => x.Action)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["ErrorsByStatusCode"] = logs.Where(x => x.IsError)
                    .GroupBy(x => x.StatusCode)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key?.ToString() ?? "Unknown", g => g.Count())
            };

            return statistics;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
