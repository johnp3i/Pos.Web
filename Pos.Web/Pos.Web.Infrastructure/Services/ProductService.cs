using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Entities.Legacy;
using Pos.Web.Infrastructure.UnitOfWork;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Product service implementation for managing product catalog operations
/// Implements caching, search, and stock availability checking
/// Now using scaffolded entities for accurate database mapping
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<ProductService> _logger;

    // Cache key constants
    private const string CacheKeyPrefix = "products";
    private const string CatalogCacheKey = $"{CacheKeyPrefix}:catalog";
    private const string CategoriesCacheKey = $"{CacheKeyPrefix}:categories";
    private const string FavoritesCacheKey = $"{CacheKeyPrefix}:favorites";
    
    // Cache expiration times
    private static readonly TimeSpan CatalogCacheExpiration = TimeSpan.FromHours(1);
    private static readonly TimeSpan CategoryCacheExpiration = TimeSpan.FromHours(2);
    private static readonly TimeSpan SearchCacheExpiration = TimeSpan.FromMinutes(15);

    public ProductService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IFeatureFlagService featureFlagService,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetProductCatalogAsync(bool includeUnavailable = false)
    {
        try
        {
            var cacheKey = $"{CatalogCacheKey}:{includeUnavailable}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Loading product catalog from database (includeUnavailable: {IncludeUnavailable})", includeUnavailable);
                
                var products = await _unitOfWork.Products.GetAllAsync();
                
                if (!includeUnavailable)
                {
                    products = products.Where(p => p.IsActive).ToList();
                }
                
                var productDtos = products.Select(MapToDto).OrderBy(p => p.DisplayOrder).ToList();
                
                _logger.LogInformation("Loaded {Count} products from database", productDtos.Count);
                
                return productDtos;
            }, CatalogCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product catalog");
            throw new ServiceException("Failed to load product catalog", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId, bool includeUnavailable = false)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}:category:{categoryId}:{includeUnavailable}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Loading products for category {CategoryId} from database", categoryId);
                
                var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
                
                if (!includeUnavailable)
                {
                    products = products.Where(p => p.IsActive).ToList();
                }
                
                var productDtos = products.Select(MapToDto).OrderBy(p => p.DisplayOrder).ToList();
                
                _logger.LogInformation("Loaded {Count} products for category {CategoryId}", productDtos.Count, categoryId);
                
                return productDtos;
            }, CatalogCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products for category {CategoryId}", categoryId);
            throw new ServiceException($"Failed to load products for category {categoryId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm, int? categoryId = null, bool includeUnavailable = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<ProductDto>();
            }

            var cacheKey = $"{CacheKeyPrefix}:search:{searchTerm}:{categoryId}:{includeUnavailable}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Searching products with term '{SearchTerm}' (categoryId: {CategoryId})", searchTerm, categoryId);
                
                var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
                
                // Filter by category if specified
                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value);
                }
                
                if (!includeUnavailable)
                {
                    products = products.Where(p => p.IsActive);
                }
                
                var productDtos = products.Select(MapToDto).OrderBy(p => p.Name).ToList();
                
                _logger.LogInformation("Found {Count} products matching '{SearchTerm}'", productDtos.Count, searchTerm);
                
                return productDtos;
            }, SearchCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term '{SearchTerm}'", searchTerm);
            throw new ServiceException($"Failed to search products with term '{searchTerm}'", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        try
        {
            _logger.LogDebug("Getting product by ID: {ProductId}", productId);
            
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return null;
            }
            
            return MapToDto(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", productId);
            throw new ServiceException($"Failed to get product {productId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            _logger.LogDebug("Getting product by barcode: {Barcode}", barcode);
            
            var product = await _unitOfWork.Products.GetProductByBarcodeAsync(barcode);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found with barcode: {Barcode}", barcode);
                return null;
            }
            
            return MapToDto(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by barcode: {Barcode}", barcode);
            throw new ServiceException($"Failed to get product by barcode '{barcode}'", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
    {
        try
        {
            _logger.LogDebug("Checking stock availability for product {ProductId}, quantity {Quantity}", productId, quantity);
            
            // Check if stock tracking is enabled via feature flag
            var isStockTrackingEnabled = await _featureFlagService.IsEnabledAsync("StockTracking");
            
            if (!isStockTrackingEnabled)
            {
                _logger.LogDebug("Stock tracking is disabled, returning true");
                return true;
            }
            
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return false;
            }
            
            // Legacy database doesn't have stock quantity tracking
            // Just check if product is active
            var isAvailable = product.IsActive;
            
            _logger.LogDebug("Stock availability for product {ProductId}: {IsAvailable}", 
                productId, isAvailable);
            
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId}", productId);
            throw new ServiceException($"Failed to check stock availability for product {productId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, bool>> CheckStockAvailabilityBatchAsync(Dictionary<int, int> items)
    {
        try
        {
            _logger.LogInformation("Checking stock availability for {Count} products", items.Count);
            
            var results = new Dictionary<int, bool>();
            
            foreach (var item in items)
            {
                var isAvailable = await CheckStockAvailabilityAsync(item.Key, item.Value);
                results[item.Key] = isAvailable;
            }
            
            var unavailableCount = results.Count(r => !r.Value);
            _logger.LogInformation("Stock check complete: {Available} available, {Unavailable} unavailable", 
                results.Count - unavailableCount, unavailableCount);
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for batch");
            throw new ServiceException("Failed to check stock availability for batch", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            return await _cacheService.GetOrCreateAsync(CategoriesCacheKey, async () =>
            {
                _logger.LogInformation("Loading categories from database");
                
                var categories = await _unitOfWork.Products.GetCategoriesAsync();
                
                var categoryDtos = categories.Select(MapCategoryToDto).OrderBy(c => c.DisplayOrder).ToList();
                
                _logger.LogInformation("Loaded {Count} categories from database", categoryDtos.Count);
                
                return categoryDtos;
            }, CategoryCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            throw new ServiceException("Failed to load categories", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetFavoriteProductsAsync(int limit = 20)
    {
        try
        {
            var cacheKey = $"{FavoritesCacheKey}:{limit}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Loading favorite products from database (limit: {Limit})", limit);
                
                var products = await _unitOfWork.Products.GetFavoriteProductsAsync();
                
                var productDtos = products.Take(limit).Select(MapToDto).ToList();
                
                _logger.LogInformation("Loaded {Count} favorite products", productDtos.Count);
                
                return productDtos;
            }, CatalogCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading favorite products");
            throw new ServiceException("Failed to load favorite products", ex);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync()
    {
        try
        {
            _logger.LogInformation("Invalidating product catalog cache");
            
            await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}:*");
            
            _logger.LogInformation("Product catalog cache invalidated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating product catalog cache");
            throw new ServiceException("Failed to invalidate product catalog cache", ex);
        }
    }

    /// <summary>
    /// Map a CategoryItem entity to a ProductDto
    /// Maps legacy database columns to DTO properties
    /// </summary>
    private static ProductDto MapToDto(CategoryItem item)
    {
        return new ProductDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Summary, // Use Summary as description
            CategoryId = item.CategoryId,
            Category = item.Category != null ? MapCategoryToDto(item.Category) : null,
            Price = item.Cost, // Cost is the price
            ImageUrl = item.ImagePath,
            Barcode = item.LabelCode,
            IsAvailable = item.IsActive,
            IsInStock = item.IsActive, // Assume active = in stock
            StockQuantity = 999, // Default (legacy DB doesn't track stock)
            DisplayOrder = item.DisplayOrder,
            IsFavorite = item.IsFreeDrinkApplied // Use IsFreeDrinkApplied as favorite indicator
        };
    }

    /// <summary>
    /// Map a Category entity to a CategoryDto
    /// </summary>
    private static CategoryDto MapCategoryToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = null, // Legacy database doesn't have description for categories
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }
}

/// <summary>
/// Custom exception for service layer errors
/// </summary>
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
