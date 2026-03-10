using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Customer repository interface with customer-specific methods
/// Following JDS repository design guidelines
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Search customers by name or phone (fuzzy search)
    /// </summary>
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    
    /// <summary>
    /// Get customer with addresses
    /// </summary>
    Task<Customer?> GetCustomerWithAddressesAsync(int customerId);
    
    /// <summary>
    /// Get customer by phone number
    /// </summary>
    Task<Customer?> GetCustomerByPhoneAsync(string phone);
    
    /// <summary>
    /// Check if customer with same name and phone already exists
    /// </summary>
    Task<bool> CheckDuplicateCustomerAsync(string name, string phone, int? excludeCustomerId = null);
    
    /// <summary>
    /// Get customers with loyalty points above threshold
    /// </summary>
    Task<IEnumerable<Customer>> GetCustomersWithLoyaltyPointsAsync(int minPoints);
    
    /// <summary>
    /// Get recently active customers (ordered in last N days)
    /// </summary>
    Task<IEnumerable<Customer>> GetRecentlyActiveCustomersAsync(int days = 30);
    
    /// <summary>
    /// Get top customers by total spent
    /// </summary>
    Task<IEnumerable<Customer>> GetTopCustomersBySpentAsync(int topCount = 10);
}
