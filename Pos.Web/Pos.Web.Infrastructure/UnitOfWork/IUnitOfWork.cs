using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Repositories;

namespace Pos.Web.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating multiple repositories in a single transaction
/// Following JDS repository design guidelines with async/await and transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository accessors for domain entities
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    
    // Specialized repository accessors for web schema entities
    IOrderLockRepository OrderLocks { get; }
    IFeatureFlagRepository FeatureFlags { get; }
    ISyncQueueRepository SyncQueues { get; }
    IAuditLogRepository ApiAuditLogs { get; }
    
    // Transaction management methods
    
    /// <summary>
    /// Save all changes to the database
    /// Returns the number of affected entities
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Begin a new database transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitAsync();
    
    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackAsync();
}
