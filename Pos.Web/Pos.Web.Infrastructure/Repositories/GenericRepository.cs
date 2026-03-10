using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Generic repository base class implementing common CRUD operations
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly PosDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(PosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Add new entity
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Update existing entity
    /// </summary>
    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    public virtual async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Check if entity exists
    /// </summary>
    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get count of entities matching predicate
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            if (predicate == null)
            {
                return await _dbSet.CountAsync();
            }
            return await _dbSet.CountAsync(predicate);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
