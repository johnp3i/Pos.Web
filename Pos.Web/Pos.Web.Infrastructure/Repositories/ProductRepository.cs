using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities.Legacy;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with product-specific methods
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// Now using scaffolded CategoryItem entity for accurate database mapping
/// </summary>
public class ProductRepository : GenericRepository<CategoryItem>, IProductRepository
{
    public ProductRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get products by category ID
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Search products by name or barcode
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> SearchProductsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActiveProductsAsync();
            }

            var normalizedSearch = searchTerm.Trim().ToLower();
            
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                    (p.Name.ToLower().Contains(normalizedSearch) ||
                     (p.LabelCode != null && p.LabelCode.Contains(normalizedSearch)) ||
                     (p.Summary != null && p.Summary.ToLower().Contains(normalizedSearch))))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get product with stock information
    /// </summary>
    public async Task<CategoryItem?> GetProductWithStockAsync(int productId)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all active products (available and in stock)
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> GetActiveProductsAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get favorite/featured products
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> GetFavoriteProductsAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFreeDrinkApplied)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get products with low stock (below threshold)
    /// Note: Legacy database doesn't have stock quantity tracking
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            // Legacy database doesn't track stock quantity
            // Return empty list for now
            return await Task.FromResult(Enumerable.Empty<CategoryItem>());
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get product by barcode (LabelCode in legacy database)
    /// </summary>
    public async Task<CategoryItem?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            var normalizedBarcode = barcode.Trim();
            
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.LabelCode == normalizedBarcode);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get products ordered by display order
    /// </summary>
    public async Task<IEnumerable<CategoryItem>> GetProductsOrderedAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        try
        {
            // Query Categories table from legacy database
            // Use AsNoTracking for read-only query performance
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
            
            return categories;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
