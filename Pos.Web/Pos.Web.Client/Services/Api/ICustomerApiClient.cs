using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for customer operations
/// </summary>
public interface ICustomerApiClient
{
    /// <summary>
    /// Search for customers by name or phone
    /// </summary>
    Task<List<CustomerDto>> SearchCustomersAsync(string query);
    
    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    Task<CustomerDto> GetCustomerAsync(int customerId);
    
    /// <summary>
    /// Create a new customer
    /// </summary>
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customer);
    
    /// <summary>
    /// Get customer order history
    /// </summary>
    Task<List<OrderDto>> GetCustomerHistoryAsync(int customerId);
    
    /// <summary>
    /// Get recent customers (last 10)
    /// </summary>
    Task<List<CustomerDto>> GetRecentCustomersAsync();
}
