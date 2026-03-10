using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Store.Order;

/// <summary>
/// Actions for order state management
/// </summary>
public static class OrderActions
{
    // ===== Current Order Actions =====
    
    /// <summary>
    /// Initialize a new order
    /// </summary>
    public record InitializeNewOrderAction(int UserId, ServiceType ServiceType, byte? TableNumber = null);
    
    /// <summary>
    /// Add an item to the current order
    /// </summary>
    public record AddItemToOrderAction(ProductDto Product, int Quantity = 1, string? Notes = null);
    
    /// <summary>
    /// Remove an item from the current order
    /// </summary>
    public record RemoveItemFromOrderAction(int OrderItemId);
    
    /// <summary>
    /// Update the quantity of an item in the current order
    /// </summary>
    public record UpdateItemQuantityAction(int OrderItemId, int NewQuantity);
    
    /// <summary>
    /// Update item notes
    /// </summary>
    public record UpdateItemNotesAction(int OrderItemId, string? Notes);
    
    /// <summary>
    /// Add extra to an order item
    /// </summary>
    public record AddItemExtraAction(int OrderItemId, OrderItemExtraDto Extra);
    
    /// <summary>
    /// Remove extra from an order item
    /// </summary>
    public record RemoveItemExtraAction(int OrderItemId, int ExtraId);
    
    /// <summary>
    /// Add flavor to an order item
    /// </summary>
    public record AddItemFlavorAction(int OrderItemId, OrderItemFlavorDto Flavor);
    
    /// <summary>
    /// Remove flavor from an order item
    /// </summary>
    public record RemoveItemFlavorAction(int OrderItemId, int FlavorId);
    
    /// <summary>
    /// Set customer for the current order
    /// </summary>
    public record SetOrderCustomerAction(CustomerDto? Customer);
    
    /// <summary>
    /// Set order notes
    /// </summary>
    public record SetOrderNotesAction(string? Notes, bool IsPrintable = false);
    
    /// <summary>
    /// Set service type for the current order
    /// </summary>
    public record SetServiceTypeAction(ServiceType ServiceType, byte? TableNumber = null);
    
    /// <summary>
    /// Update order notes
    /// </summary>
    public record UpdateOrderNotesAction(string? Notes, bool IsPrintable = false);
    
    /// <summary>
    /// Apply discount to the current order
    /// </summary>
    public record ApplyDiscountAction(decimal? DiscountPercentage = null, decimal? DiscountAmount = null, string? Reason = null);
    
    /// <summary>
    /// Apply voucher to the current order
    /// </summary>
    public record ApplyVoucherAction(int VoucherId);
    
    /// <summary>
    /// Clear the current order
    /// </summary>
    public record ClearCurrentOrderAction();
    
    /// <summary>
    /// Recalculate order totals
    /// </summary>
    public record RecalculateOrderTotalsAction();
    
    // ===== Order Creation Actions =====
    
    /// <summary>
    /// Create order (submit to kitchen/payment)
    /// </summary>
    public record CreateOrderAction();
    
    /// <summary>
    /// Order created successfully
    /// </summary>
    public record CreateOrderSuccessAction(int OrderId);
    
    /// <summary>
    /// Failed to create order
    /// </summary>
    public record CreateOrderFailureAction(string ErrorMessage);
    
    // ===== Pending Orders Actions =====
    
    /// <summary>
    /// Load pending orders from API
    /// </summary>
    public record LoadPendingOrdersAction();
    
    /// <summary>
    /// Pending orders loaded successfully
    /// </summary>
    public record LoadPendingOrdersSuccessAction(List<PendingOrderDto> PendingOrders);
    
    /// <summary>
    /// Failed to load pending orders
    /// </summary>
    public record LoadPendingOrdersFailureAction(string ErrorMessage);
    
    /// <summary>
    /// Load a pending order into current order
    /// </summary>
    public record LoadPendingOrderAction(int PendingOrderId);
    
    /// <summary>
    /// Pending order loaded successfully
    /// </summary>
    public record LoadPendingOrderSuccessAction(OrderDto Order);
    
    /// <summary>
    /// Failed to load pending order
    /// </summary>
    public record LoadPendingOrderFailureAction(string ErrorMessage);
    
    /// <summary>
    /// Save current order as pending
    /// </summary>
    public record SaveAsPendingAction();
    
    /// <summary>
    /// Order saved as pending successfully
    /// </summary>
    public record SaveAsPendingSuccessAction(int PendingOrderId);
    
    /// <summary>
    /// Failed to save order as pending
    /// </summary>
    public record SaveAsPendingFailureAction(string ErrorMessage);
    
    /// <summary>
    /// Delete a pending order
    /// </summary>
    public record DeletePendingOrderAction(int PendingOrderId);
    
    /// <summary>
    /// Pending order deleted successfully
    /// </summary>
    public record DeletePendingOrderSuccessAction(int PendingOrderId);
    
    /// <summary>
    /// Failed to delete pending order
    /// </summary>
    public record DeletePendingOrderFailureAction(string ErrorMessage);
    
    // ===== Order Lock Actions =====
    
    /// <summary>
    /// Order locked by another user
    /// </summary>
    public record OrderLockedAction(int OrderId, string LockedByUser);
    
    /// <summary>
    /// Order unlocked
    /// </summary>
    public record OrderUnlockedAction(int OrderId);
}
