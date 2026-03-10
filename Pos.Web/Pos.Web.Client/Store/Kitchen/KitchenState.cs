using Fluxor;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Store.Kitchen;

/// <summary>
/// State for kitchen display management
/// </summary>
[FeatureState]
public record KitchenState
{
    /// <summary>
    /// All active orders in the kitchen
    /// </summary>
    public List<OrderDto> ActiveOrders { get; init; } = new();
    
    /// <summary>
    /// Orders filtered by status
    /// </summary>
    public List<OrderDto> FilteredOrders { get; init; } = new();
    
    /// <summary>
    /// Current status filter (null = all statuses)
    /// </summary>
    public OrderStatus? StatusFilter { get; init; }
    
    /// <summary>
    /// Whether orders are being loaded
    /// </summary>
    public bool IsLoadingOrders { get; init; }
    
    /// <summary>
    /// Whether an order status is being updated
    /// </summary>
    public bool IsUpdatingStatus { get; init; }
    
    /// <summary>
    /// ID of the order being updated
    /// </summary>
    public int? UpdatingOrderId { get; init; }
    
    /// <summary>
    /// Whether SignalR is connected
    /// </summary>
    public bool IsConnected { get; init; }
    
    /// <summary>
    /// Error message if any operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Timestamp of last refresh
    /// </summary>
    public DateTime? LastRefreshedAt { get; init; }
}
