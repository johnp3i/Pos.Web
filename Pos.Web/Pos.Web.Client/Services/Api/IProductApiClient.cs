using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for product operations
/// </summary>
public interface IProductApiClient
{
    /// <summary>
    /// Get all products
    /// </summary>
    Task<List<ProductDto>> GetProductsAsync();
    
    /// <summary>
    /// Get all categories
    /// </summary>
    Task<List<CategoryDto>> GetCategoriesAsync();
    
    /// <summary>
    /// Get products by category
    /// </summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    
    /// <summary>
    /// Search products by name or barcode
    /// </summary>
    Task<List<ProductDto>> SearchProductsAsync(string query);
}
