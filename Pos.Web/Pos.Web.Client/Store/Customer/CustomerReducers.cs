using Fluxor;

namespace Pos.Web.Client.Store.Customer;

/// <summary>
/// Reducers for customer state
/// </summary>
public static class CustomerReducers
{
    // ===== Customer Selection Reducers =====
    
    [ReducerMethod]
    public static CustomerState ReduceSelectCustomerAction(CustomerState state, CustomerActions.SelectCustomerAction action)
    {
        return state with
        {
            SelectedCustomer = action.Customer,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceClearSelectedCustomerAction(CustomerState state, CustomerActions.ClearSelectedCustomerAction action)
    {
        return state with
        {
            SelectedCustomer = null,
            CustomerHistory = new List<Pos.Web.Shared.DTOs.OrderDto>(),
            ErrorMessage = null
        };
    }
    
    // ===== Customer Search Reducers =====
    
    [ReducerMethod]
    public static CustomerState ReduceSearchCustomersAction(CustomerState state, CustomerActions.SearchCustomersAction action)
    {
        return state with
        {
            IsSearching = true,
            SearchQuery = action.Query,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceSearchCustomersSuccessAction(CustomerState state, CustomerActions.SearchCustomersSuccessAction action)
    {
        return state with
        {
            SearchResults = action.Results,
            SearchQuery = action.Query,
            IsSearching = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceSearchCustomersFailureAction(CustomerState state, CustomerActions.SearchCustomersFailureAction action)
    {
        return state with
        {
            IsSearching = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceClearSearchResultsAction(CustomerState state, CustomerActions.ClearSearchResultsAction action)
    {
        return state with
        {
            SearchResults = new List<Pos.Web.Shared.DTOs.CustomerDto>(),
            SearchQuery = null,
            ErrorMessage = null
        };
    }
    
    // ===== Customer Creation Reducers =====
    
    [ReducerMethod]
    public static CustomerState ReduceCreateCustomerAction(CustomerState state, CustomerActions.CreateCustomerAction action)
    {
        return state with
        {
            IsCreatingCustomer = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceCreateCustomerSuccessAction(CustomerState state, CustomerActions.CreateCustomerSuccessAction action)
    {
        return state with
        {
            SelectedCustomer = action.Customer,
            IsCreatingCustomer = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceCreateCustomerFailureAction(CustomerState state, CustomerActions.CreateCustomerFailureAction action)
    {
        return state with
        {
            IsCreatingCustomer = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Customer History Reducers =====
    
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomerHistoryAction(CustomerState state, CustomerActions.LoadCustomerHistoryAction action)
    {
        return state with
        {
            IsLoadingHistory = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomerHistorySuccessAction(CustomerState state, CustomerActions.LoadCustomerHistorySuccessAction action)
    {
        return state with
        {
            CustomerHistory = action.History,
            IsLoadingHistory = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomerHistoryFailureAction(CustomerState state, CustomerActions.LoadCustomerHistoryFailureAction action)
    {
        return state with
        {
            IsLoadingHistory = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Recent Customers Reducers =====
    
    [ReducerMethod]
    public static CustomerState ReduceLoadRecentCustomersSuccessAction(CustomerState state, CustomerActions.LoadRecentCustomersSuccessAction action)
    {
        return state with
        {
            RecentCustomers = action.RecentCustomers,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceLoadRecentCustomersFailureAction(CustomerState state, CustomerActions.LoadRecentCustomersFailureAction action)
    {
        return state with
        {
            ErrorMessage = action.ErrorMessage
        };
    }
}
