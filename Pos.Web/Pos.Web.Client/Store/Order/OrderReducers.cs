using Fluxor;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Store.Order;

/// <summary>
/// Reducers for order state
/// </summary>
public static class OrderReducers
{
    [ReducerMethod]
    public static OrderState ReduceInitializeNewOrderAction(OrderState state, OrderActions.InitializeNewOrderAction action)
    {
        var newOrder = new OrderDto
        {
            UserId = action.UserId,
            ServiceType = action.ServiceType,
            TableNumber = action.TableNumber,
            Status = OrderStatus.Pending,
            Items = new List<OrderItemDto>(),
            CreatedAt = DateTime.Now,
            Subtotal = 0,
            TaxAmount = 0,
            TotalAmount = 0
        };
        
        return state with
        {
            CurrentOrder = newOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceAddItemToOrderAction(OrderState state, OrderActions.AddItemToOrderAction action)
    {
        // Initialize a new order if one doesn't exist
        var currentOrder = state.CurrentOrder;
        if (currentOrder == null)
        {
            currentOrder = new OrderDto
            {
                UserId = 0, // Will be set when user is authenticated
                ServiceType = ServiceType.DineIn, // Default service type
                Status = OrderStatus.Pending,
                Items = new List<OrderItemDto>(),
                CreatedAt = DateTime.Now,
                Subtotal = 0,
                TaxAmount = 0,
                TotalAmount = 0
            };
        }
        
        var newItem = new OrderItemDto
        {
            Id = -(currentOrder.Items.Count + 1), // Temporary negative ID for new items
            ProductId = action.Product.Id,
            Product = action.Product,
            Quantity = action.Quantity,
            UnitPrice = action.Product.Price,
            TotalPrice = action.Product.Price * action.Quantity,
            Notes = action.Notes,
            Extras = new List<OrderItemExtraDto>(),
            Flavors = new List<OrderItemFlavorDto>()
        };
        
        var updatedOrder = CloneOrder(currentOrder);
        updatedOrder.Items.Add(newItem);
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceRemoveItemFromOrderAction(OrderState state, OrderActions.RemoveItemFromOrderAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.Items = updatedOrder.Items.Where(i => i.Id != action.OrderItemId).ToList();
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceUpdateItemQuantityAction(OrderState state, OrderActions.UpdateItemQuantityAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Quantity = action.NewQuantity;
            item.TotalPrice = item.UnitPrice * action.NewQuantity;
        }
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceUpdateItemNotesAction(OrderState state, OrderActions.UpdateItemNotesAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Notes = action.Notes;
        }
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceAddItemExtraAction(OrderState state, OrderActions.AddItemExtraAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Extras.Add(action.Extra);
        }
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceRemoveItemExtraAction(OrderState state, OrderActions.RemoveItemExtraAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Extras = item.Extras.Where(e => e.Id != action.ExtraId).ToList();
        }
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceAddItemFlavorAction(OrderState state, OrderActions.AddItemFlavorAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Flavors.Add(action.Flavor);
        }
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceRemoveItemFlavorAction(OrderState state, OrderActions.RemoveItemFlavorAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        var item = updatedOrder.Items.FirstOrDefault(i => i.Id == action.OrderItemId);
        if (item != null)
        {
            item.Flavors = item.Flavors.Where(f => f.Id != action.FlavorId).ToList();
        }
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSetOrderCustomerAction(OrderState state, OrderActions.SetOrderCustomerAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.CustomerId = action.Customer?.Id;
        updatedOrder.Customer = action.Customer;
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSetServiceTypeAction(OrderState state, OrderActions.SetServiceTypeAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.ServiceType = action.ServiceType;
        updatedOrder.TableNumber = action.TableNumber;
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceUpdateOrderNotesAction(OrderState state, OrderActions.UpdateOrderNotesAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.Notes = action.Notes;
        updatedOrder.IsNotesPrintable = action.IsPrintable;
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSetOrderNotesAction(OrderState state, OrderActions.SetOrderNotesAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.Notes = action.Notes;
        updatedOrder.IsNotesPrintable = action.IsPrintable;
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceApplyDiscountAction(OrderState state, OrderActions.ApplyDiscountAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.DiscountPercentage = action.DiscountPercentage;
        updatedOrder.DiscountAmount = action.DiscountAmount;
        
        return state with
        {
            CurrentOrder = RecalculateTotals(updatedOrder),
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceApplyVoucherAction(OrderState state, OrderActions.ApplyVoucherAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        var updatedOrder = CloneOrder(state.CurrentOrder);
        updatedOrder.VoucherId = action.VoucherId;
        
        return state with
        {
            CurrentOrder = updatedOrder,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceClearCurrentOrderAction(OrderState state, OrderActions.ClearCurrentOrderAction action)
    {
        return state with
        {
            CurrentOrder = null,
            ErrorMessage = null,
            LockedOrderId = null,
            LockedByUser = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceRecalculateOrderTotalsAction(OrderState state, OrderActions.RecalculateOrderTotalsAction action)
    {
        if (state.CurrentOrder == null)
            return state;
        
        return state with
        {
            CurrentOrder = RecalculateTotals(CloneOrder(state.CurrentOrder))
        };
    }
    
    // ===== Order Creation Reducers =====
    
    [ReducerMethod]
    public static OrderState ReduceCreateOrderAction(OrderState state, OrderActions.CreateOrderAction action)
    {
        return state with
        {
            IsCreatingOrder = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceCreateOrderSuccessAction(OrderState state, OrderActions.CreateOrderSuccessAction action)
    {
        return state with
        {
            IsCreatingOrder = false,
            CurrentOrder = null, // Clear current order after successful creation
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceCreateOrderFailureAction(OrderState state, OrderActions.CreateOrderFailureAction action)
    {
        return state with
        {
            IsCreatingOrder = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Pending Orders Reducers =====
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrdersAction(OrderState state, OrderActions.LoadPendingOrdersAction action)
    {
        return state with
        {
            IsLoadingPendingOrders = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrdersSuccessAction(OrderState state, OrderActions.LoadPendingOrdersSuccessAction action)
    {
        return state with
        {
            PendingOrders = action.PendingOrders,
            IsLoadingPendingOrders = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrdersFailureAction(OrderState state, OrderActions.LoadPendingOrdersFailureAction action)
    {
        return state with
        {
            IsLoadingPendingOrders = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrderSuccessAction(OrderState state, OrderActions.LoadPendingOrderSuccessAction action)
    {
        return state with
        {
            CurrentOrder = action.Order,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrderFailureAction(OrderState state, OrderActions.LoadPendingOrderFailureAction action)
    {
        return state with
        {
            ErrorMessage = action.ErrorMessage
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSaveAsPendingAction(OrderState state, OrderActions.SaveAsPendingAction action)
    {
        return state with
        {
            IsSavingOrder = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSaveAsPendingSuccessAction(OrderState state, OrderActions.SaveAsPendingSuccessAction action)
    {
        return state with
        {
            IsSavingOrder = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceSaveAsPendingFailureAction(OrderState state, OrderActions.SaveAsPendingFailureAction action)
    {
        return state with
        {
            IsSavingOrder = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceDeletePendingOrderSuccessAction(OrderState state, OrderActions.DeletePendingOrderSuccessAction action)
    {
        var updatedPendingOrders = state.PendingOrders.Where(p => p.Id != action.PendingOrderId).ToList();
        
        return state with
        {
            PendingOrders = updatedPendingOrders,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceDeletePendingOrderFailureAction(OrderState state, OrderActions.DeletePendingOrderFailureAction action)
    {
        return state with
        {
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Order Lock Reducers =====
    
    [ReducerMethod]
    public static OrderState ReduceOrderLockedAction(OrderState state, OrderActions.OrderLockedAction action)
    {
        return state with
        {
            LockedOrderId = action.OrderId,
            LockedByUser = action.LockedByUser
        };
    }
    
    [ReducerMethod]
    public static OrderState ReduceOrderUnlockedAction(OrderState state, OrderActions.OrderUnlockedAction action)
    {
        if (state.LockedOrderId == action.OrderId)
        {
            return state with
            {
                LockedOrderId = null,
                LockedByUser = null
            };
        }
        
        return state;
    }
    
    // ===== Helper Methods =====
    
    /// <summary>
    /// Clone an order (shallow clone with new lists)
    /// </summary>
    private static OrderDto CloneOrder(OrderDto order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Customer = order.Customer,
            UserId = order.UserId,
            ServiceType = order.ServiceType,
            TableNumber = order.TableNumber,
            Status = order.Status,
            Items = new List<OrderItemDto>(order.Items),
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
    /// Recalculate order totals (subtotal, tax, discounts, total)
    /// </summary>
    private static OrderDto RecalculateTotals(OrderDto order)
    {
        // Calculate subtotal from items
        var subtotal = order.Items.Sum(item => item.TotalPrice);
        
        // Add extras to subtotal
        foreach (var item in order.Items)
        {
            subtotal += item.Extras.Sum(e => e.Price * item.Quantity);
        }
        
        // Calculate tax (assuming 10% tax rate - should be configurable)
        const decimal taxRate = 0.10m;
        var taxAmount = subtotal * taxRate;
        
        // Calculate discount
        decimal discountTotal = 0;
        if (order.DiscountPercentage.HasValue)
        {
            discountTotal = subtotal * (order.DiscountPercentage.Value / 100);
        }
        else if (order.DiscountAmount.HasValue)
        {
            discountTotal = order.DiscountAmount.Value;
        }
        
        // Calculate total
        var totalAmount = subtotal + taxAmount - discountTotal;
        
        // Ensure total is not negative
        if (totalAmount < 0)
            totalAmount = 0;
        
        order.Subtotal = subtotal;
        order.TaxAmount = taxAmount;
        order.TotalAmount = totalAmount;
        
        return order;
    }
}
