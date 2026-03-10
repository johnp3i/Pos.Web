using System.Linq.Expressions;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations
/// Following JDS repository design guidelines
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Add new entity
    /// </summary>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Update existing entity
    /// </summary>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Delete entity
    /// </summary>
    Task DeleteAsync(T entity);
    
    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(int id);
    
    /// <summary>
    /// Get count of entities matching predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
