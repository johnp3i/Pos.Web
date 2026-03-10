using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Customer service interface for managing customer operations
/// Provides customer search, creation, and history retrieval
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Search customers by name or phone number with fuzzy matching
    /// Results are cached for 5 minutes
    /// </summary>
    /// <param name="searchTerm">Search term (name or phone)</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <returns>List of matching customers</returns>
    Task<List<CustomerDto>> SearchCustomersAsync(string searchTerm, int limit = 20);

    /// <summary>
    /// Get a single customer by ID with addresses
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer details or null if not found</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(int customerId);

    /// <summary>
    /// Get a customer by phone number
    /// </summary>
    /// <param name="phone">Phone number</param>
    /// <returns>Customer details or null if not found</returns>
    Task<CustomerDto?> GetCustomerByPhoneAsync(string phone);

    /// <summary>
    /// Create a new customer with duplicate detection
    /// </summary>
    /// <param name="customerDto">Customer data</param>
    /// <param name="userId">User ID creating the customer</param>
    /// <returns>Created customer with ID</returns>
    /// <exception cref="DuplicateCustomerException">Thrown when customer with same name and phone already exists</exception>
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto, int userId);

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerDto">Updated customer data</param>
    /// <param name="userId">User ID making the update</param>
    /// <returns>Updated customer</returns>
    Task<CustomerDto> UpdateCustomerAsync(int customerId, CustomerDto customerDto, int userId);

    /// <summary>
    /// Get customer order history with statistics
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="limit">Maximum number of orders to return</param>
    /// <returns>Customer history with orders and statistics</returns>
    Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId, int limit = 50);

    /// <summary>
    /// Add loyalty points to a customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="points">Points to add (can be negative to deduct)</param>
    /// <param name="userId">User ID making the change</param>
    /// <param name="reason">Reason for the points change</param>
    Task AddLoyaltyPointsAsync(int customerId, int points, int userId, string reason);

    /// <summary>
    /// Get recently active customers (ordered in last N days)
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    /// <param name="limit">Maximum number of customers to return</param>
    /// <returns>List of recently active customers</returns>
    Task<List<CustomerDto>> GetRecentlyActiveCustomersAsync(int days = 30, int limit = 20);

    /// <summary>
    /// Get top customers by total spent
    /// </summary>
    /// <param name="topCount">Number of top customers to return</param>
    /// <returns>List of top customers</returns>
    Task<List<CustomerDto>> GetTopCustomersAsync(int topCount = 10);

    /// <summary>
    /// Deactivate a customer (soft delete)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="userId">User ID making the change</param>
    Task DeactivateCustomerAsync(int customerId, int userId);

    /// <summary>
    /// Reactivate a deactivated customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="userId">User ID making the change</param>
    Task ReactivateCustomerAsync(int customerId, int userId);

    /// <summary>
    /// Invalidate customer cache
    /// Call this when customers are updated externally
    /// </summary>
    Task InvalidateCacheAsync();
}

/// <summary>
/// Customer history DTO with orders and statistics
/// </summary>
public class CustomerHistoryDto
{
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderDto> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? FirstOrderDate { get; set; }
}

/// <summary>
/// Exception thrown when attempting to create a duplicate customer
/// </summary>
public class DuplicateCustomerException : Exception
{
    public int ExistingCustomerId { get; }

    public DuplicateCustomerException(string message, int existingCustomerId) : base(message)
    {
        ExistingCustomerId = existingCustomerId;
    }
}
