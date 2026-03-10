using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// API client for product operations
/// </summary>
public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiClient> _logger;
    
    public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        try
        {
            _logger.LogDebug("Loading all products");
            
            // API returns paginated result wrapped in ApiResponse
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedResult<ProductDto>>>("/api/products?pageSize=1000");
            
            if (response == null || !response.Success || response.Data == null)
            {
                _logger.LogWarning("Products API returned null or unsuccessful response, returning empty list");
                return new List<ProductDto>();
            }
            
            _logger.LogDebug("Loaded {Count} products", response.Data.Items.Count);
            return response.Data.Items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading products");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            _logger.LogDebug("Loading categories");
            
            // API returns list wrapped in ApiResponse
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("/api/products/categories");
            
            if (response == null || !response.Success || response.Data == null)
            {
                _logger.LogWarning("Categories API returned null or unsuccessful response, returning empty list");
                return new List<CategoryDto>();
            }
            
            _logger.LogDebug("Loaded {Count} categories", response.Data.Count);
            return response.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading categories");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            _logger.LogDebug("Loading products for category {CategoryId}", categoryId);
            
            var response = await _httpClient.GetFromJsonAsync<List<ProductDto>>($"/api/products/category/{categoryId}");
            
            if (response == null)
            {
                _logger.LogWarning("Products by category API returned null for category {CategoryId}, returning empty list", categoryId);
                return new List<ProductDto>();
            }
            
            _logger.LogDebug("Loaded {Count} products for category {CategoryId}", response.Count, categoryId);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading products for category {CategoryId}", categoryId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products for category {CategoryId}", categoryId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<ProductDto>> SearchProductsAsync(string query)
    {
        try
        {
            _logger.LogDebug("Searching products with query: {Query}", query);
            
            var response = await _httpClient.GetFromJsonAsync<List<ProductDto>>($"/api/products/search?q={Uri.EscapeDataString(query)}");
            
            if (response == null)
            {
                _logger.LogWarning("Product search returned null for query: {Query}, returning empty list", query);
                return new List<ProductDto>();
            }
            
            _logger.LogDebug("Found {Count} products for query: {Query}", response.Count, query);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error searching products with query: {Query}", query);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", query);
            throw;
        }
    }
}
