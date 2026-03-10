using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Product service interface for managing product catalog operations
/// Provides product retrieval, search, and stock availability checking
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Get the complete product catalog with caching
    /// Results are cached for 1 hour to improve performance
    /// </summary>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>List of all products</returns>
    Task<List<ProductDto>> GetProductCatalogAsync(bool includeUnavailable = false);

    /// <summary>
    /// Get products by category with caching
    /// </summary>
    /// <param name="categoryId">Category ID to filter by</param>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>List of products in the specified category</returns>
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId, bool includeUnavailable = false);

    /// <summary>
    /// Search products by name or barcode
    /// </summary>
    /// <param name="searchTerm">Search term (name or barcode)</param>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>List of matching products</returns>
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm, int? categoryId = null, bool includeUnavailable = false);

    /// <summary>
    /// Get a single product by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Product details or null if not found</returns>
    Task<ProductDto?> GetProductByIdAsync(int productId);

    /// <summary>
    /// Get a product by barcode
    /// </summary>
    /// <param name="barcode">Product barcode</param>
    /// <returns>Product details or null if not found</returns>
    Task<ProductDto?> GetProductByBarcodeAsync(string barcode);

    /// <summary>
    /// Check stock availability for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to check</param>
    /// <returns>True if sufficient stock is available</returns>
    Task<bool> CheckStockAvailabilityAsync(int productId, int quantity);

    /// <summary>
    /// Check stock availability for multiple products
    /// </summary>
    /// <param name="items">Dictionary of product IDs and quantities</param>
    /// <returns>Dictionary of product IDs and availability status</returns>
    Task<Dictionary<int, bool>> CheckStockAvailabilityBatchAsync(Dictionary<int, int> items);

    /// <summary>
    /// Get all product categories
    /// </summary>
    /// <returns>List of all categories</returns>
    Task<List<CategoryDto>> GetCategoriesAsync();

    /// <summary>
    /// Get favorite/featured products
    /// </summary>
    /// <param name="limit">Maximum number of products to return</param>
    /// <returns>List of favorite products</returns>
    Task<List<ProductDto>> GetFavoriteProductsAsync(int limit = 20);

    /// <summary>
    /// Invalidate product catalog cache
    /// Call this when products are updated in the database
    /// </summary>
    Task InvalidateCacheAsync();
}
