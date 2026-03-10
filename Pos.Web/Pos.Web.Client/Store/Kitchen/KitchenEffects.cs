using Fluxor;
using Pos.Web.Client.Services.Api;

namespace Pos.Web.Client.Store.Kitchen;

/// <summary>
/// Effects for kitchen state management
/// </summary>
public class KitchenEffects
{
    private readonly IKitchenApiClient _kitchenApiClient;
    private readonly ILogger<KitchenEffects> _logger;

    public KitchenEffects(
        IKitchenApiClient kitchenApiClient,
        ILogger<KitchenEffects> logger)
    {
        _kitchenApiClient = kitchenApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Load kitchen orders from API
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadKitchenOrdersAction(KitchenActions.LoadKitchenOrdersAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Loading kitchen orders from API");
            
            var orders = await _kitchenApiClient.GetActiveOrdersAsync();
            
            dispatcher.Dispatch(new KitchenActions.LoadKitchenOrdersSuccessAction(orders));
            
            _logger.LogInformation("Successfully loaded {Count} kitchen orders", orders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load kitchen orders");
            dispatcher.Dispatch(new KitchenActions.LoadKitchenOrdersFailureAction(ex.Message));
        }
    }

    /// <summary>
    /// Update order status via API
    /// </summary>
    [EffectMethod]
    public async Task HandleUpdateOrderStatusAction(KitchenActions.UpdateOrderStatusAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} status to {Status}", action.OrderId, action.NewStatus);
            
            await _kitchenApiClient.UpdateOrderStatusAsync(action.OrderId, action.NewStatus);
            
            dispatcher.Dispatch(new KitchenActions.UpdateOrderStatusSuccessAction(action.OrderId, action.NewStatus));
            
            _logger.LogInformation("Successfully updated order {OrderId} status to {Status}", action.OrderId, action.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update order {OrderId} status to {Status}", action.OrderId, action.NewStatus);
            dispatcher.Dispatch(new KitchenActions.UpdateOrderStatusFailureAction(action.OrderId, ex.Message));
        }
    }
}