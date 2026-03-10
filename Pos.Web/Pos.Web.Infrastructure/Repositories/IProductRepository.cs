using Pos.Web.Infrastructure.Entities.Legacy;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Product repository interface with product-specific methods
/// Following JDS repository design guidelines
/// Now using scaffolded CategoryItem entity for accurate database mapping
/// </summary>
public interface IProductRepository : IRepository<CategoryItem>
{
    /// <summary>
    /// Get products by category ID
    /// </summary>
    Task<IEnumerable<CategoryItem>> GetProductsByCategoryAsync(int categoryId);
    
    /// <summary>
    /// Search products by name or barcode
    /// </summary>
    Task<IEnumerable<CategoryItem>> SearchProductsAsync(string searchTerm);
    
    /// <summary>
    /// Get product with stock information
    /// </summary>
    Task<CategoryItem?> GetProductWithStockAsync(int productId);
    
    /// <summary>
    /// Get all active products (available and in stock)
    /// </summary>
    Task<IEnumerable<CategoryItem>> GetActiveProductsAsync();
    
    /// <summary>
    /// Get favorite/featured products
    /// </summary>
    Task<IEnumerable<CategoryItem>> GetFavoriteProductsAsync();
    
    /// <summary>
    /// Get products with low stock (below threshold)
    /// </summary>
    Task<IEnumerable<CategoryItem>> GetLowStockProductsAsync(int threshold = 10);
    
    /// <summary>
    /// Get product by barcode
    /// </summary>
    Task<CategoryItem?> GetProductByBarcodeAsync(string barcode);
    
    /// <summary>
    /// Get products ordered by display order
    /// </summary>
    Task<IEnumerable<CategoryItem>> GetProductsOrderedAsync();
    
    /// <summary>
    /// Get all categories
    /// </summary>
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
