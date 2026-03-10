using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Products controller for managing POS product catalog
/// Handles product retrieval, search, and category management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 200)</param>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [AllowAnonymous] // Allow anonymous access for development/testing
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool includeUnavailable = false)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(ApiResponse<object>.Error("Page number must be greater than 0"));
            }

            if (pageSize < 1 || pageSize > 200)
            {
                pageSize = 50;
            }

            _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}, IncludeUnavailable: {IncludeUnavailable}",
                page, pageSize, includeUnavailable);

            var allProducts = await _productService.GetProductCatalogAsync(includeUnavailable);

            // Apply pagination
            var totalCount = allProducts.Count;
            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PaginatedResult<ProductDto>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            _logger.LogInformation("Retrieved {Count} products (Page {Page} of {TotalPages})",
                products.Count, page, result.TotalPages);

            return Ok(ApiResponse<PaginatedResult<ProductDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProducts: Error retrieving products");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving products", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Search products by name or barcode
    /// </summary>
    /// <param name="q">Search query (name or barcode)</param>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>List of matching products</returns>
    /// <response code="200">Products found</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("search")]
    [AllowAnonymous] // Allow anonymous access for development/testing
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string q,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool includeUnavailable = false)
    {
        try
        {
            // Validate search query
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiResponse<object>.Error("Search query cannot be empty"));
            }

            if (q.Length < 2)
            {
                return BadRequest(ApiResponse<object>.Error("Search query must be at least 2 characters"));
            }

            _logger.LogInformation("Searching products with query: {Query}, CategoryId: {CategoryId}, IncludeUnavailable: {IncludeUnavailable}",
                q, categoryId, includeUnavailable);

            var products = await _productService.SearchProductsAsync(q, categoryId, includeUnavailable);

            _logger.LogInformation("Found {Count} products matching query: {Query}", products.Count, q);
            return Ok(ApiResponse<List<ProductDto>>.Ok(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchProducts: Error searching products with query: {Query}", q);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while searching products", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    /// <returns>List of all categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("categories")]
    [AllowAnonymous] // Allow anonymous access for development/testing
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            _logger.LogInformation("Getting all product categories");

            var categories = await _productService.GetCategoriesAsync();

            _logger.LogInformation("Retrieved {Count} categories", categories.Count);
            return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCategories: Error retrieving categories");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving categories", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="includeUnavailable">Whether to include unavailable products</param>
    /// <returns>List of products in the specified category</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("category/{id}")]
    [AllowAnonymous] // Allow anonymous access for development/testing
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductsByCategory(int id, [FromQuery] bool includeUnavailable = false)
    {
        try
        {
            _logger.LogInformation("Getting products for category {CategoryId}, IncludeUnavailable: {IncludeUnavailable}",
                id, includeUnavailable);

            var products = await _productService.GetProductsByCategoryAsync(id, includeUnavailable);

            if (products == null || products.Count == 0)
            {
                _logger.LogWarning("No products found for category {CategoryId}", id);
                // Return empty list instead of 404 - category might exist but have no products
                return Ok(ApiResponse<List<ProductDto>>.Ok(new List<ProductDto>()));
            }

            _logger.LogInformation("Retrieved {Count} products for category {CategoryId}", products.Count, id);
            return Ok(ApiResponse<List<ProductDto>>.Ok(products));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "GetProductsByCategory: Category not found - {CategoryId}", id);
            return NotFound(ApiResponse<object>.Error($"Category with ID {id} not found", StatusCodes.Status404NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProductsByCategory: Error retrieving products for category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving products", StatusCodes.Status500InternalServerError));
        }
    }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
