using Fluxor;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Store.ProductCatalog;

/// <summary>
/// State for product catalog management
/// </summary>
[FeatureState]
public record ProductCatalogState
{
    /// <summary>
    /// All products in the catalog
    /// </summary>
    public List<ProductDto> Products { get; init; } = new();
    
    /// <summary>
    /// All categories
    /// </summary>
    public List<CategoryDto> Categories { get; init; } = new();
    
    /// <summary>
    /// Currently selected category (null = all categories)
    /// </summary>
    public int? SelectedCategoryId { get; init; }
    
    /// <summary>
    /// Filtered products based on category and search
    /// </summary>
    public List<ProductDto> FilteredProducts { get; init; } = new();
    
    /// <summary>
    /// Current search query
    /// </summary>
    public string? SearchQuery { get; init; }
    
    /// <summary>
    /// Whether products are being loaded
    /// </summary>
    public bool IsLoadingProducts { get; init; }
    
    /// <summary>
    /// Whether categories are being loaded
    /// </summary>
    public bool IsLoadingCategories { get; init; }
    
    /// <summary>
    /// Timestamp of last catalog load (for cache management)
    /// </summary>
    public DateTime? LastLoadedAt { get; init; }
    
    /// <summary>
    /// Error message if any operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}
