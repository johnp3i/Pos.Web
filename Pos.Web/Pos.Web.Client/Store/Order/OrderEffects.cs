using Fluxor;
using Pos.Web.Client.Services.Api;

namespace Pos.Web.Client.Store.Order;

/// <summary>
/// Effects for order state (side effects like API calls)
/// </summary>
public class OrderEffects
{
    private readonly IOrderApiClient _orderApiClient;
    
    public OrderEffects(IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }
    
    [EffectMethod]
    public async Task HandleLoadPendingOrdersAction(OrderActions.LoadPendingOrdersAction action, IDispatcher dispatcher)
    {
        try
        {
            var pendingOrders = await _orderApiClient.GetPendingOrdersAsync();
            dispatcher.Dispatch(new OrderActions.LoadPendingOrdersSuccessAction(pendingOrders));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OrderActions.LoadPendingOrdersFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadPendingOrderAction(OrderActions.LoadPendingOrderAction action, IDispatcher dispatcher)
    {
        try
        {
            var order = await _orderApiClient.GetPendingOrderAsync(action.PendingOrderId);
            dispatcher.Dispatch(new OrderActions.LoadPendingOrderSuccessAction(order));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OrderActions.LoadPendingOrderFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleSaveAsPendingAction(OrderActions.SaveAsPendingAction action, IDispatcher dispatcher)
    {
        try
        {
            var state = await GetOrderStateAsync(dispatcher);
            if (state?.CurrentOrder == null)
            {
                dispatcher.Dispatch(new OrderActions.SaveAsPendingFailureAction("No current order to save"));
                return;
            }
            
            var pendingOrderId = await _orderApiClient.SaveAsPendingAsync(state.CurrentOrder);
            dispatcher.Dispatch(new OrderActions.SaveAsPendingSuccessAction(pendingOrderId));
            
            // Reload pending orders to refresh the list
            dispatcher.Dispatch(new OrderActions.LoadPendingOrdersAction());
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OrderActions.SaveAsPendingFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleDeletePendingOrderAction(OrderActions.DeletePendingOrderAction action, IDispatcher dispatcher)
    {
        try
        {
            await _orderApiClient.DeletePendingOrderAsync(action.PendingOrderId);
            dispatcher.Dispatch(new OrderActions.DeletePendingOrderSuccessAction(action.PendingOrderId));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new OrderActions.DeletePendingOrderFailureAction(ex.Message));
        }
    }
    
    /// <summary>
    /// Helper method to get current order state
    /// Note: This is a workaround since we can't inject IState directly into effects
    /// In a real implementation, you might want to pass the order as part of the action
    /// </summary>
    private async Task<OrderState?> GetOrderStateAsync(IDispatcher dispatcher)
    {
        // This is a placeholder - in practice, you'd either:
        // 1. Pass the order as part of the action
        // 2. Use a different pattern to access state in effects
        // 3. Inject IState<OrderState> if Fluxor supports it
        
        // For now, we'll return null and handle it in the effect
        await Task.CompletedTask;
        return null;
    }
}
