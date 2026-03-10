using Fluxor;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Store.Order;

/// <summary>
/// State for order management
/// </summary>
[FeatureState]
public record OrderState
{
    /// <summary>
    /// Current order being created/edited
    /// </summary>
    public OrderDto? CurrentOrder { get; init; }
    
    /// <summary>
    /// List of pending orders (saved but not completed)
    /// </summary>
    public List<PendingOrderDto> PendingOrders { get; init; } = new();
    
    /// <summary>
    /// Whether pending orders are being loaded
    /// </summary>
    public bool IsLoadingPendingOrders { get; init; }
    
    /// <summary>
    /// Whether current order is being saved
    /// </summary>
    public bool IsSavingOrder { get; init; }
    
    /// <summary>
    /// Whether current order is being created/submitted
    /// </summary>
    public bool IsCreatingOrder { get; init; }
    
    /// <summary>
    /// Error message if any operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// ID of the order that is currently locked by another user
    /// </summary>
    public int? LockedOrderId { get; init; }
    
    /// <summary>
    /// Name of the user who locked the order
    /// </summary>
    public string? LockedByUser { get; init; }
}
