using Fluxor;
using Pos.Web.Client.Services.Api;

namespace Pos.Web.Client.Store.Customer;

/// <summary>
/// Effects for customer state (side effects like API calls)
/// </summary>
public class CustomerEffects
{
    private readonly ICustomerApiClient _customerApiClient;
    
    public CustomerEffects(ICustomerApiClient customerApiClient)
    {
        _customerApiClient = customerApiClient;
    }
    
    [EffectMethod]
    public async Task HandleSearchCustomersAction(CustomerActions.SearchCustomersAction action, IDispatcher dispatcher)
    {
        try
        {
            var results = await _customerApiClient.SearchCustomersAsync(action.Query);
            dispatcher.Dispatch(new CustomerActions.SearchCustomersSuccessAction(results, action.Query));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CustomerActions.SearchCustomersFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleCreateCustomerAction(CustomerActions.CreateCustomerAction action, IDispatcher dispatcher)
    {
        try
        {
            var customer = await _customerApiClient.CreateCustomerAsync(action.Customer);
            dispatcher.Dispatch(new CustomerActions.CreateCustomerSuccessAction(customer));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CustomerActions.CreateCustomerFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadCustomerHistoryAction(CustomerActions.LoadCustomerHistoryAction action, IDispatcher dispatcher)
    {
        try
        {
            var history = await _customerApiClient.GetCustomerHistoryAsync(action.CustomerId);
            dispatcher.Dispatch(new CustomerActions.LoadCustomerHistorySuccessAction(history));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CustomerActions.LoadCustomerHistoryFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadRecentCustomersAction(CustomerActions.LoadRecentCustomersAction action, IDispatcher dispatcher)
    {
        try
        {
            var recentCustomers = await _customerApiClient.GetRecentCustomersAsync();
            dispatcher.Dispatch(new CustomerActions.LoadRecentCustomersSuccessAction(recentCustomers));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CustomerActions.LoadRecentCustomersFailureAction(ex.Message));
        }
    }
}
