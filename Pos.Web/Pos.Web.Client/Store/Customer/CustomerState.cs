using Fluxor;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Store.Customer;

/// <summary>
/// State for customer management
/// </summary>
[FeatureState]
public record CustomerState
{
    /// <summary>
    /// Currently selected customer
    /// </summary>
    public CustomerDto? SelectedCustomer { get; init; }
    
    /// <summary>
    /// Customer search results
    /// </summary>
    public List<CustomerDto> SearchResults { get; init; } = new();
    
    /// <summary>
    /// Recent customers (for quick selection)
    /// </summary>
    public List<CustomerDto> RecentCustomers { get; init; } = new();
    
    /// <summary>
    /// Whether a search is in progress
    /// </summary>
    public bool IsSearching { get; init; }
    
    /// <summary>
    /// Whether customer is being created
    /// </summary>
    public bool IsCreatingCustomer { get; init; }
    
    /// <summary>
    /// Whether customer history is being loaded
    /// </summary>
    public bool IsLoadingHistory { get; init; }
    
    /// <summary>
    /// Customer order history
    /// </summary>
    public List<OrderDto> CustomerHistory { get; init; } = new();
    
    /// <summary>
    /// Current search query
    /// </summary>
    public string? SearchQuery { get; init; }
    
    /// <summary>
    /// Error message if any operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}
