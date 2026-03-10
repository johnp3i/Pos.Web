using Fluxor;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Store.Kitchen;

/// <summary>
/// Reducers for kitchen state
/// </summary>
public static class KitchenReducers
{
    // ===== Order Loading Reducers =====
    
    [ReducerMethod]
    public static KitchenState ReduceLoadKitchenOrdersAction(KitchenState state, KitchenActions.LoadKitchenOrdersAction action)
    {
        return state with
        {
            IsLoadingOrders = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static KitchenState ReduceLoadKitchenOrdersSuccessAction(KitchenState state, KitchenActions.LoadKitchenOrdersSuccessAction action)
    {
        var newState = state with
        {
            ActiveOrders = action.Orders,
            IsLoadingOrders = false,
            LastRefreshedAt = DateTime.Now,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceLoadKitchenOrdersFailureAction(KitchenState state, KitchenActions.LoadKitchenOrdersFailureAction action)
    {
        return state with
        {
            IsLoadingOrders = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Order Status Update Reducers =====
    
    [ReducerMethod]
    public static KitchenState ReduceUpdateOrderStatusAction(KitchenState state, KitchenActions.UpdateOrderStatusAction action)
    {
        return state with
        {
            IsUpdatingStatus = true,
            UpdatingOrderId = action.OrderId,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static KitchenState ReduceUpdateOrderStatusSuccessAction(KitchenState state, KitchenActions.UpdateOrderStatusSuccessAction action)
    {
        // Update the order status in the active orders list
        var updatedOrders = state.ActiveOrders.Select(order =>
        {
            if (order.Id == action.OrderId)
            {
                var clonedOrder = CloneOrder(order);
                clonedOrder.Status = action.NewStatus;
                return clonedOrder;
            }
            return order;
        }).ToList();
        
        // Remove completed or delivered orders from active list
        if (action.NewStatus == OrderStatus.Completed || action.NewStatus == OrderStatus.Delivered)
        {
            updatedOrders = updatedOrders.Where(o => o.Id != action.OrderId).ToList();
        }
        
        var newState = state with
        {
            ActiveOrders = updatedOrders,
            IsUpdatingStatus = false,
            UpdatingOrderId = null,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceUpdateOrderStatusFailureAction(KitchenState state, KitchenActions.UpdateOrderStatusFailureAction action)
    {
        return state with
        {
            IsUpdatingStatus = false,
            UpdatingOrderId = null,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Filtering Reducers =====
    
    [ReducerMethod]
    public static KitchenState ReduceFilterByStatusAction(KitchenState state, KitchenActions.FilterByStatusAction action)
    {
        var newState = state with
        {
            StatusFilter = action.Status,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceClearFilterAction(KitchenState state, KitchenActions.ClearFilterAction action)
    {
        var newState = state with
        {
            StatusFilter = null,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    // ===== SignalR Real-time Reducers =====
    
    [ReducerMethod]
    public static KitchenState ReduceNewOrderReceivedAction(KitchenState state, KitchenActions.NewOrderReceivedAction action)
    {
        // Add new order to the beginning of the list
        var updatedOrders = new List<Pos.Web.Shared.DTOs.OrderDto> { action.Order };
        updatedOrders.AddRange(state.ActiveOrders);
        
        var newState = state with
        {
            ActiveOrders = updatedOrders,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceOrderStatusChangedAction(KitchenState state, KitchenActions.OrderStatusChangedAction action)
    {
        // Update the order status in the active orders list
        var updatedOrders = state.ActiveOrders.Select(order =>
        {
            if (order.Id == action.OrderId)
            {
                var clonedOrder = CloneOrder(order);
                clonedOrder.Status = action.NewStatus;
                return clonedOrder;
            }
            return order;
        }).ToList();
        
        // Remove completed or delivered orders from active list
        if (action.NewStatus == OrderStatus.Completed || action.NewStatus == OrderStatus.Delivered)
        {
            updatedOrders = updatedOrders.Where(o => o.Id != action.OrderId).ToList();
        }
        
        var newState = state with
        {
            ActiveOrders = updatedOrders,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceOrderRemovedAction(KitchenState state, KitchenActions.OrderRemovedAction action)
    {
        var updatedOrders = state.ActiveOrders.Where(o => o.Id != action.OrderId).ToList();
        
        var newState = state with
        {
            ActiveOrders = updatedOrders,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static KitchenState ReduceConnectionStatusChangedAction(KitchenState state, KitchenActions.ConnectionStatusChangedAction action)
    {
        return state with
        {
            IsConnected = action.IsConnected
        };
    }
    
    // ===== Helper Methods =====
    
    /// <summary>
    /// Clone an order (shallow clone with new lists)
    /// </summary>
    private static Pos.Web.Shared.DTOs.OrderDto CloneOrder(Pos.Web.Shared.DTOs.OrderDto order)
    {
        return new Pos.Web.Shared.DTOs.OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Customer = order.Customer,
            UserId = order.UserId,
            ServiceType = order.ServiceType,
            TableNumber = order.TableNumber,
            Status = order.Status,
            Items = new List<Pos.Web.Shared.DTOs.OrderItemDto>(order.Items),
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountPercentage = order.DiscountPercentage,
            DiscountAmount = order.DiscountAmount,
            VoucherId = order.VoucherId,
            TotalAmount = order.TotalAmount,
            AmountPaid = order.AmountPaid,
            ChangeAmount = order.ChangeAmount,
            Notes = order.Notes,
            IsNotesPrintable = order.IsNotesPrintable,
            ScheduledTime = order.ScheduledTime,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CompletedAt = order.CompletedAt
        };
    }
    
    /// <summary>
    /// Apply status filter to active orders
    /// </summary>
    private static KitchenState ApplyFilters(KitchenState state)
    {
        var filtered = state.ActiveOrders.AsEnumerable();
        
        // Filter by status if specified
        if (state.StatusFilter.HasValue)
        {
            filtered = filtered.Where(o => o.Status == state.StatusFilter.Value);
        }
        
        // Sort by creation time (oldest first - FIFO)
        filtered = filtered.OrderBy(o => o.CreatedAt);
        
        return state with
        {
            FilteredOrders = filtered.ToList()
        };
    }
}
