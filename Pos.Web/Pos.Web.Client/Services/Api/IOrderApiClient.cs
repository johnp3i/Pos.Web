using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for order operations
/// </summary>
public interface IOrderApiClient
{
    /// <summary>
    /// Get all pending orders
    /// </summary>
    Task<List<PendingOrderDto>> GetPendingOrdersAsync();
    
    /// <summary>
    /// Get a specific pending order by ID
    /// </summary>
    Task<OrderDto> GetPendingOrderAsync(int pendingOrderId);
    
    /// <summary>
    /// Save an order as pending
    /// </summary>
    Task<int> SaveAsPendingAsync(OrderDto order);
    
    /// <summary>
    /// Delete a pending order
    /// </summary>
    Task DeletePendingOrderAsync(int pendingOrderId);
    
    /// <summary>
    /// Create a new order (complete checkout)
    /// </summary>
    Task<int> CreateOrderAsync(OrderDto order);
    
    /// <summary>
    /// Update an existing order
    /// </summary>
    Task UpdateOrderAsync(int orderId, OrderDto order);
}
