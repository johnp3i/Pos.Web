using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Order repository interface with order-specific methods
/// Following JDS repository design guidelines
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Get all pending orders (not completed)
    /// </summary>
    Task<IEnumerable<Order>> GetPendingOrdersAsync();
    
    /// <summary>
    /// Get orders by customer ID with order items
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Get orders within date range
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Get order with all related data (items, customer, user)
    /// </summary>
    Task<Order?> GetOrderWithItemsAsync(int orderId);
    
    /// <summary>
    /// Get orders by table number
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByTableNumberAsync(byte tableNumber);
    
    /// <summary>
    /// Get orders by user ID (cashier/waiter)
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    
    /// <summary>
    /// Get today's orders
    /// </summary>
    Task<IEnumerable<Order>> GetTodaysOrdersAsync();
    
    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
    
    /// <summary>
    /// Get orders by customer ID with limit (for customer history)
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, int limit);
    
    /// <summary>
    /// Update an existing order
    /// </summary>
    void Update(Order order);
}
