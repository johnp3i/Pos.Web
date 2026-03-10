using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Store.ProductCatalog;

/// <summary>
/// Actions for product catalog state management
/// </summary>
public static class ProductCatalogActions
{
    // ===== Product Loading Actions =====
    
    /// <summary>
    /// Load all products from API
    /// </summary>
    public record LoadProductsAction(bool ForceRefresh = false);
    
    /// <summary>
    /// Products loaded successfully
    /// </summary>
    public record LoadProductsSuccessAction(List<ProductDto> Products);
    
    /// <summary>
    /// Products loading failed
    /// </summary>
    public record LoadProductsFailureAction(string ErrorMessage);
    
    // ===== Category Loading Actions =====
    
    /// <summary>
    /// Load all categories from API
    /// </summary>
    public record LoadCategoriesAction();
    
    /// <summary>
    /// Categories loaded successfully
    /// </summary>
    public record LoadCategoriesSuccessAction(List<CategoryDto> Categories);
    
    /// <summary>
    /// Categories loading failed
    /// </summary>
    public record LoadCategoriesFailureAction(string ErrorMessage);
    
    // ===== Filtering Actions =====
    
    /// <summary>
    /// Filter products by category
    /// </summary>
    public record FilterByCategoryAction(int? CategoryId);
    
    /// <summary>
    /// Search products by name or description
    /// </summary>
    public record SearchProductsAction(string Query);
    
    /// <summary>
    /// Clear search and show all products
    /// </summary>
    public record ClearSearchAction();
    
    /// <summary>
    /// Apply filters (internal action after filter changes)
    /// </summary>
    public record ApplyFiltersAction();
}
