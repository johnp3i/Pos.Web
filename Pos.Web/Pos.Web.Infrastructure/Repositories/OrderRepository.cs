using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Order repository implementation with order-specific methods
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all pending orders (not completed)
    /// </summary>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == "Pending" || o.Status == "InProgress")
                .OrderBy(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get orders by customer ID with order items
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get orders within date range
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.TimeStamp >= fromDate && o.TimeStamp <= toDate)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get order with all related data (items, customer, user)
    /// </summary>
    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.Addresses)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(o => o.ID == orderId);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get orders by table number
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByTableNumberAsync(byte tableNumber)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.TableNumber == tableNumber)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get orders by user ID (cashier/waiter)
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get today's orders
    /// </summary>
    public async Task<IEnumerable<Order>> GetTodaysOrdersAsync()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.TimeStamp >= today && o.TimeStamp < tomorrow)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get orders by status
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.TimeStamp)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    /// <summary>
    /// Get orders by customer ID with limit (for customer history)
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, int limit)
    {
        try
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.TimeStamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    /// <summary>
    /// Update an existing order
    /// </summary>
    public void Update(Order order)
    {
        try
        {
            _dbSet.Update(order);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
