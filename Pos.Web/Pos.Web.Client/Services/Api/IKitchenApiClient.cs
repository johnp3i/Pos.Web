using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for kitchen operations
/// </summary>
public interface IKitchenApiClient
{
    /// <summary>
    /// Get all active kitchen orders
    /// </summary>
    Task<List<OrderDto>> GetActiveOrdersAsync();
    
    /// <summary>
    /// Update order status
    /// </summary>
    Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    
    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
}
