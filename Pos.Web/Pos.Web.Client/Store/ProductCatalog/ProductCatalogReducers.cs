using Fluxor;

namespace Pos.Web.Client.Store.ProductCatalog;

/// <summary>
/// Reducers for product catalog state
/// </summary>
public static class ProductCatalogReducers
{
    // ===== Product Loading Reducers =====
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadProductsAction(ProductCatalogState state, ProductCatalogActions.LoadProductsAction action)
    {
        return state with
        {
            IsLoadingProducts = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadProductsSuccessAction(ProductCatalogState state, ProductCatalogActions.LoadProductsSuccessAction action)
    {
        var newState = state with
        {
            Products = action.Products,
            IsLoadingProducts = false,
            LastLoadedAt = DateTime.Now,
            ErrorMessage = null
        };
        
        // Apply current filters to new products
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadProductsFailureAction(ProductCatalogState state, ProductCatalogActions.LoadProductsFailureAction action)
    {
        return state with
        {
            IsLoadingProducts = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Category Loading Reducers =====
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadCategoriesAction(ProductCatalogState state, ProductCatalogActions.LoadCategoriesAction action)
    {
        return state with
        {
            IsLoadingCategories = true,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadCategoriesSuccessAction(ProductCatalogState state, ProductCatalogActions.LoadCategoriesSuccessAction action)
    {
        return state with
        {
            Categories = action.Categories,
            IsLoadingCategories = false,
            ErrorMessage = null
        };
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceLoadCategoriesFailureAction(ProductCatalogState state, ProductCatalogActions.LoadCategoriesFailureAction action)
    {
        return state with
        {
            IsLoadingCategories = false,
            ErrorMessage = action.ErrorMessage
        };
    }
    
    // ===== Filtering Reducers =====
    
    [ReducerMethod]
    public static ProductCatalogState ReduceFilterByCategoryAction(ProductCatalogState state, ProductCatalogActions.FilterByCategoryAction action)
    {
        var newState = state with
        {
            SelectedCategoryId = action.CategoryId,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceSearchProductsAction(ProductCatalogState state, ProductCatalogActions.SearchProductsAction action)
    {
        var newState = state with
        {
            SearchQuery = action.Query,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceClearSearchAction(ProductCatalogState state, ProductCatalogActions.ClearSearchAction action)
    {
        var newState = state with
        {
            SearchQuery = null,
            SelectedCategoryId = null,
            ErrorMessage = null
        };
        
        return ApplyFilters(newState);
    }
    
    [ReducerMethod]
    public static ProductCatalogState ReduceApplyFiltersAction(ProductCatalogState state, ProductCatalogActions.ApplyFiltersAction action)
    {
        return ApplyFilters(state);
    }
    
    // ===== Helper Methods =====
    
    /// <summary>
    /// Apply current filters (category and search) to products
    /// </summary>
    private static ProductCatalogState ApplyFilters(ProductCatalogState state)
    {
        var filtered = state.Products.AsEnumerable();
        
        // Filter by category
        if (state.SelectedCategoryId.HasValue)
        {
            filtered = filtered.Where(p => p.CategoryId == state.SelectedCategoryId.Value);
        }
        
        // Filter by search query
        if (!string.IsNullOrWhiteSpace(state.SearchQuery))
        {
            var query = state.SearchQuery.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Name.ToLowerInvariant().Contains(query) ||
                (p.Description != null && p.Description.ToLowerInvariant().Contains(query)) ||
                (p.Barcode != null && p.Barcode.Contains(query))
            );
        }
        
        // Filter out unavailable products
        filtered = filtered.Where(p => p.IsAvailable);
        
        // Sort by display order, then by name
        filtered = filtered.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name);
        
        return state with
        {
            FilteredProducts = filtered.ToList()
        };
    }
}
