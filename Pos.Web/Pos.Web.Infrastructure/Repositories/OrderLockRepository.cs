using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Order lock repository for web.OrderLocks table
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class OrderLockRepository : GenericRepository<OrderLock>, IOrderLockRepository
{
    public OrderLockRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get active lock for a specific order
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<OrderLock?> GetActiveLockByOrderIdAsync(int orderId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.OrderID == orderId && x.IsActive && x.LockExpiresAt > DateTime.UtcNow);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all active locks for a specific user
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<OrderLock>> GetActiveLocksByUserIdAsync(int userId)
    {
        try
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.UserID == userId && x.IsActive && x.LockExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.LockAcquiredAt)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all expired locks that need cleanup
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<IEnumerable<OrderLock>> GetExpiredLocksAsync()
    {
        try
        {
            return await _dbSet
                .Where(x => x.IsActive && x.LockExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Release lock for a specific order
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> ReleaseLockAsync(int orderId, int userId)
    {
        try
        {
            var lockEntity = await _dbSet
                .FirstOrDefaultAsync(x => x.OrderID == orderId && x.UserID == userId && x.IsActive);

            if (lockEntity == null)
            {
                return false;
            }

            lockEntity.IsActive = false;
            lockEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Extend lock expiration time
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<bool> ExtendLockAsync(int orderId, int userId, int additionalMinutes)
    {
        try
        {
            var lockEntity = await _dbSet
                .FirstOrDefaultAsync(x => x.OrderID == orderId && x.UserID == userId && x.IsActive);

            if (lockEntity == null || lockEntity.LockExpiresAt <= DateTime.UtcNow)
            {
                return false;
            }

            lockEntity.LockExpiresAt = lockEntity.LockExpiresAt.AddMinutes(additionalMinutes);
            lockEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Cleanup expired locks (set IsActive to false)
    /// Following JDS guideline: async/await with try/catch
    /// </summary>
    public async Task<int> CleanupExpiredLocksAsync()
    {
        try
        {
            var expiredLocks = await _dbSet
                .Where(x => x.IsActive && x.LockExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (!expiredLocks.Any())
            {
                return 0;
            }

            foreach (var lockEntity in expiredLocks)
            {
                lockEntity.IsActive = false;
                lockEntity.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return expiredLocks.Count;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
