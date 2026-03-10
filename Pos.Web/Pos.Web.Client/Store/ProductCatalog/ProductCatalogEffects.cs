using Fluxor;
using Pos.Web.Client.Services.Api;

namespace Pos.Web.Client.Store.ProductCatalog;

/// <summary>
/// Effects for product catalog state (side effects like API calls)
/// </summary>
public class ProductCatalogEffects
{
    private readonly IProductApiClient _productApiClient;
    
    public ProductCatalogEffects(IProductApiClient productApiClient)
    {
        _productApiClient = productApiClient;
    }
    
    [EffectMethod]
    public async Task HandleLoadProductsAction(ProductCatalogActions.LoadProductsAction action, IDispatcher dispatcher)
    {
        try
        {
            // TODO: Implement caching logic
            // For now, always load from API
            var products = await _productApiClient.GetProductsAsync();
            dispatcher.Dispatch(new ProductCatalogActions.LoadProductsSuccessAction(products));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ProductCatalogActions.LoadProductsFailureAction(ex.Message));
        }
    }
    
    [EffectMethod]
    public async Task HandleLoadCategoriesAction(ProductCatalogActions.LoadCategoriesAction action, IDispatcher dispatcher)
    {
        try
        {
            var categories = await _productApiClient.GetCategoriesAsync();
            dispatcher.Dispatch(new ProductCatalogActions.LoadCategoriesSuccessAction(categories));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ProductCatalogActions.LoadCategoriesFailureAction(ex.Message));
        }
    }
}
