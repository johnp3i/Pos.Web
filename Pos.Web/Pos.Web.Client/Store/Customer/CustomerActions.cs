using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Store.Customer;

/// <summary>
/// Actions for customer state management
/// </summary>
public static class CustomerActions
{
    // ===== Customer Selection Actions =====
    
    /// <summary>
    /// Select a customer
    /// </summary>
    public record SelectCustomerAction(CustomerDto? Customer);
    
    /// <summary>
    /// Clear selected customer
    /// </summary>
    public record ClearSelectedCustomerAction();
    
    // ===== Customer Search Actions =====
    
    /// <summary>
    /// Search for customers by name or phone
    /// </summary>
    public record SearchCustomersAction(string Query);
    
    /// <summary>
    /// Customer search completed successfully
    /// </summary>
    public record SearchCustomersSuccessAction(List<CustomerDto> Results, string Query);
    
    /// <summary>
    /// Customer search failed
    /// </summary>
    public record SearchCustomersFailureAction(string ErrorMessage);
    
    /// <summary>
    /// Clear search results
    /// </summary>
    public record ClearSearchResultsAction();
    
    // ===== Customer Creation Actions =====
    
    /// <summary>
    /// Create a new customer
    /// </summary>
    public record CreateCustomerAction(CustomerDto Customer);
    
    /// <summary>
    /// Customer created successfully
    /// </summary>
    public record CreateCustomerSuccessAction(CustomerDto Customer);
    
    /// <summary>
    /// Customer creation failed
    /// </summary>
    public record CreateCustomerFailureAction(string ErrorMessage);
    
    // ===== Customer History Actions =====
    
    /// <summary>
    /// Load customer order history
    /// </summary>
    public record LoadCustomerHistoryAction(int CustomerId);
    
    /// <summary>
    /// Customer history loaded successfully
    /// </summary>
    public record LoadCustomerHistorySuccessAction(List<OrderDto> History);
    
    /// <summary>
    /// Customer history loading failed
    /// </summary>
    public record LoadCustomerHistoryFailureAction(string ErrorMessage);
    
    // ===== Recent Customers Actions =====
    
    /// <summary>
    /// Load recent customers
    /// </summary>
    public record LoadRecentCustomersAction();
    
    /// <summary>
    /// Recent customers loaded successfully
    /// </summary>
    public record LoadRecentCustomersSuccessAction(List<CustomerDto> RecentCustomers);
    
    /// <summary>
    /// Recent customers loading failed
    /// </summary>
    public record LoadRecentCustomersFailureAction(string ErrorMessage);
}
