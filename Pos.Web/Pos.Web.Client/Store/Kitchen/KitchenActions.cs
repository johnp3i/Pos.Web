using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Store.Kitchen;

/// <summary>
/// Actions for kitchen state management
/// </summary>
public static class KitchenActions
{
    // ===== Order Loading Actions =====
    
    /// <summary>
    /// Load active kitchen orders from API
    /// </summary>
    public record LoadKitchenOrdersAction();
    
    /// <summary>
    /// Kitchen orders loaded successfully
    /// </summary>
    public record LoadKitchenOrdersSuccessAction(List<OrderDto> Orders);
    
    /// <summary>
    /// Kitchen orders loading failed
    /// </summary>
    public record LoadKitchenOrdersFailureAction(string ErrorMessage);
    
    // ===== Order Status Update Actions =====
    
    /// <summary>
    /// Update order status (Preparing, Ready, Delivered)
    /// </summary>
    public record UpdateOrderStatusAction(int OrderId, OrderStatus NewStatus);
    
    /// <summary>
    /// Order status updated successfully
    /// </summary>
    public record UpdateOrderStatusSuccessAction(int OrderId, OrderStatus NewStatus);
    
    /// <summary>
    /// Order status update failed
    /// </summary>
    public record UpdateOrderStatusFailureAction(int OrderId, string ErrorMessage);
    
    // ===== Filtering Actions =====
    
    /// <summary>
    /// Filter orders by status
    /// </summary>
    public record FilterByStatusAction(OrderStatus? Status);
    
    /// <summary>
    /// Clear status filter
    /// </summary>
    public record ClearFilterAction();
    
    // ===== SignalR Real-time Actions =====
    
    /// <summary>
    /// New order received via SignalR
    /// </summary>
    public record NewOrderReceivedAction(OrderDto Order);
    
    /// <summary>
    /// Order status changed via SignalR
    /// </summary>
    public record OrderStatusChangedAction(int OrderId, OrderStatus NewStatus);
    
    /// <summary>
    /// Order removed from kitchen (completed or cancelled)
    /// </summary>
    public record OrderRemovedAction(int OrderId);
    
    /// <summary>
    /// SignalR connection status changed
    /// </summary>
    public record ConnectionStatusChangedAction(bool IsConnected);
}
