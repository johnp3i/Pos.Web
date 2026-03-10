using Microsoft.EntityFrameworkCore.Storage;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Repositories;

namespace Pos.Web.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation coordinating multiple repositories in a single transaction
/// Following JDS repository design guidelines with async/await, try/catch, and transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PosDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Lazy-initialized repositories
    private IOrderRepository? _orders;
    private ICustomerRepository? _customers;
    private IProductRepository? _products;
    private IOrderLockRepository? _orderLocks;
    private IFeatureFlagRepository? _featureFlags;
    private ISyncQueueRepository? _syncQueues;
    private IAuditLogRepository? _apiAuditLogs;
    
    private bool _disposed = false;

    public UnitOfWork(PosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Order repository accessor with lazy initialization
    /// </summary>
    public IOrderRepository Orders
    {
        get
        {
            _orders ??= new OrderRepository(_context);
            return _orders;
        }
    }

    /// <summary>
    /// Customer repository accessor with lazy initialization
    /// </summary>
    public ICustomerRepository Customers
    {
        get
        {
            _customers ??= new CustomerRepository(_context);
            return _customers;
        }
    }

    /// <summary>
    /// Product repository accessor with lazy initialization
    /// </summary>
    public IProductRepository Products
    {
        get
        {
            _products ??= new ProductRepository(_context);
            return _products;
        }
    }

    /// <summary>
    /// OrderLock repository accessor with lazy initialization
    /// </summary>
    public IOrderLockRepository OrderLocks
    {
        get
        {
            _orderLocks ??= new OrderLockRepository(_context);
            return _orderLocks;
        }
    }

    /// <summary>
    /// FeatureFlag repository accessor with lazy initialization
    /// </summary>
    public IFeatureFlagRepository FeatureFlags
    {
        get
        {
            _featureFlags ??= new FeatureFlagRepository(_context);
            return _featureFlags;
        }
    }

    /// <summary>
    /// SyncQueue repository accessor with lazy initialization
    /// </summary>
    public ISyncQueueRepository SyncQueues
    {
        get
        {
            _syncQueues ??= new SyncQueueRepository(_context);
            return _syncQueues;
        }
    }

    /// <summary>
    /// ApiAuditLog repository accessor with lazy initialization
    /// </summary>
    public IAuditLogRepository ApiAuditLogs
    {
        get
        {
            _apiAuditLogs ??= new AuditLogRepository(_context);
            return _apiAuditLogs;
        }
    }

    /// <summary>
    /// Save all changes to the database
    /// Returns the number of affected entities
    /// Following JDS guideline: try/catch with rethrow
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Begin a new database transaction
    /// Following JDS guideline: async/await pattern
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Commit the current transaction
    /// Following JDS guideline: try/catch with rethrow
    /// </summary>
    public async Task CommitAsync()
    {
        try
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            await _transaction.CommitAsync();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Rollback the current transaction
    /// Following JDS guideline: try/catch with rethrow
    /// </summary>
    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            await _transaction.RollbackAsync();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Dispose of resources
    /// Following JDS guideline: proper resource disposal
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method for proper cleanup
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose transaction if still active
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                // DbContext is managed by DI container, don't dispose it here
                // The DI container will handle DbContext disposal
            }

            _disposed = true;
        }
    }
}
