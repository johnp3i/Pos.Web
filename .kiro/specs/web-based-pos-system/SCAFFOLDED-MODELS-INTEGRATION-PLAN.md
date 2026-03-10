# Scaffolded Models Integration Plan

## Current State

The POS database has been successfully scaffolded using EF Core tooling. We now have:

**Old Manual Models** (incomplete, inaccurate):
- `Pos.Web.Infrastructure/Entities/Category.cs`
- `Pos.Web.Infrastructure/Entities/Product.cs`
- Missing columns, incorrect types

**New Scaffolded Models** (accurate, complete):
- `Pos.Web.Infrastructure/Entities/Legacy/Category.cs`
- `Pos.Web.Infrastructure/Entities/Legacy/CategoryItem.cs`
- All columns present, correct types, proper relationships

## Key Differences Identified

### Category Entity

**Old Manual Model** (missing columns):
```csharp
public class Category
{
    public int ID { get; set; }
    public string Name { get; set; }
    // Missing: Description (commented out)
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
```

**Scaffolded Model** (complete):
```csharp
public partial class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime TimeStamp { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
    public int DisplayOrder { get; set; }
    public byte CategorySectionTypeId { get; set; }
    public byte? CategoryExtrasTypeId { get; set; }
    public bool HasSeperateReceipt { get; set; }
    public bool IsExtraCategory { get; set; }
    public byte CategoryOperationDepartmentId { get; set; }
    public bool IsExtrasWindowEnable { get; set; }
    public bool IsOnlineCategoryOnly { get; set; }
    public byte? ColorTypeId { get; set; }
    
    // Navigation properties
    public virtual ICollection<CategoryItem> CategoryItemCategories { get; set; }
    // ... many more
}
```

### Product/CategoryItem Entity

**Old Manual Model** (many missing columns):
```csharp
public class Product
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int CategoryID { get; set; }
    public int DisplayOrder { get; set; }
    
    // These don't exist in legacy table:
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Barcode { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFavorite { get; set; }
}
```

**Scaffolded Model** (complete):
```csharp
public partial class CategoryItem
{
    public int Id { get; set; }
    public long? ItemOnlineId { get; set; }
    public long? ItemReferenceId { get; set; }
    public int CategoryId { get; set; }
    public int? ExtrasCategoryId { get; set; }
    public string Name { get; set; }
    public DateTime TimeStamp { get; set; }
    public bool IsActive { get; set; }
    public decimal Cost { get; set; }
    public decimal? ProductVolumeInGr { get; set; }
    public decimal? ProductTotalWeightInGr { get; set; }
    public string? ImagePath { get; set; }
    public bool IsFreeDrinkApplied { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsExtraItem { get; set; }
    public byte? ColorTypeId { get; set; }
    public decimal RealCost { get; set; }
    public string? LabelCode { get; set; }
    public byte? LabelHeaderId { get; set; }
    public byte? Vatid { get; set; }
    public string? Summary { get; set; }
    public bool HasPrariorityOnReceipt { get; set; }
    public bool IsOnlineItemOnly { get; set; }
    public int? ExtrasSpecialFeaturesTypeId { get; set; }
    public int? SupplierProductId { get; set; }
    public bool IsStandAloneItem { get; set; }
    public DateTime? LastUpdate { get; set; }
    public bool IsExtraWindowForcedDisabled { get; set; }
    
    // Navigation properties
    public virtual Category Category { get; set; }
    // ... many more
}
```

## Integration Strategy

### Phase 1: Update PosDbContext (Immediate)
1. Import scaffolded entity namespace
2. Update DbSet declarations to use scaffolded types
3. Copy OnModelCreating configuration from scaffolded context
4. Keep web schema configurations

### Phase 2: Update Repositories (Immediate)
1. Update ProductRepository to use `Legacy.CategoryItem` instead of `Product`
2. Update method signatures and queries
3. Update GetCategoriesAsync to use `Legacy.Category`

### Phase 3: Create DTO Mapping Layer (Immediate)
1. Update ProductService to map from `Legacy.CategoryItem` to `ProductDto`
2. Update CategoryDto mapping to use scaffolded Category properties
3. Handle property name differences (ID vs Id, CategoryID vs CategoryId)

### Phase 4: Update Other Components (As Needed)
1. Update OrderRepository if it references products
2. Update any other services that use Category or Product
3. Update controllers if needed

### Phase 5: Cleanup (After Testing)
1. Remove old manual entity files
2. Update documentation
3. Remove commented-out code

## Property Name Mapping

### Category
| Old Property | Scaffolded Property | Notes |
|--------------|---------------------|-------|
| ID | Id | Lowercase 'd' |
| Name | Name | Same |
| DisplayOrder | DisplayOrder | Same |
| IsActive | IsActive | Same |
| (missing) | TimeStamp | New |
| (missing) | ImagePath | New |
| (missing) | CategorySectionTypeId | New |
| (missing) | CategoryOperationDepartmentId | New |
| (missing) | ColorTypeId | New |

### Product/CategoryItem
| Old Property | Scaffolded Property | Notes |
|--------------|---------------------|-------|
| ID | Id | Lowercase 'd' |
| Name | Name | Same |
| CategoryID | CategoryId | Lowercase 'i' |
| DisplayOrder | DisplayOrder | Same |
| Price | Cost | Different name! |
| IsAvailable | IsActive | Different name! |
| ImageUrl | ImagePath | Different name! |
| Barcode | LabelCode | Different name! |
| (missing) | TimeStamp | New |
| (missing) | RealCost | New |
| (missing) | Summary | New (description-like) |
| (missing) | IsFreeDrinkApplied | New |
| (missing) | ColorTypeId | New |

## Implementation Steps

### Step 1: Update PosDbContext.cs

```csharp
using Pos.Web.Infrastructure.Entities.Legacy;

public class PosDbContext : DbContext
{
    // Update DbSets to use scaffolded types
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<CategoryItem> CategoryItems { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Copy configuration from scaffolded context
        // Keep web schema configurations
    }
}
```

### Step 2: Update ProductRepository.cs

```csharp
using Pos.Web.Infrastructure.Entities.Legacy;

public class ProductRepository : GenericRepository<CategoryItem>, IProductRepository
{
    public async Task<IEnumerable<CategoryItem>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}
```

### Step 3: Update ProductService.cs

```csharp
using Pos.Web.Infrastructure.Entities.Legacy;

private static ProductDto MapToDto(CategoryItem item)
{
    return new ProductDto
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Summary, // Use Summary as description
        CategoryId = item.CategoryId,
        Price = item.Cost, // Cost is the price
        ImageUrl = item.ImagePath,
        Barcode = item.LabelCode,
        IsAvailable = item.IsActive,
        IsInStock = item.IsActive, // Assume active = in stock
        StockQuantity = 999, // Default
        DisplayOrder = item.DisplayOrder,
        IsFavorite = item.IsFreeDrinkApplied // Or use another field
    };
}

private static CategoryDto MapCategoryToDto(Category category)
{
    return new CategoryDto
    {
        Id = category.Id,
        Name = category.Name,
        Description = null, // No description in legacy table
        DisplayOrder = category.DisplayOrder,
        IsActive = category.IsActive
    };
}
```

### Step 4: Update IProductRepository.cs

```csharp
using Pos.Web.Infrastructure.Entities.Legacy;

public interface IProductRepository : IGenericRepository<CategoryItem>
{
    Task<IEnumerable<CategoryItem>> GetProductsByCategoryAsync(int categoryId);
    Task<IEnumerable<CategoryItem>> SearchProductsAsync(string searchTerm);
    Task<CategoryItem?> GetProductWithStockAsync(int productId);
    Task<IEnumerable<CategoryItem>> GetActiveProductsAsync();
    Task<IEnumerable<CategoryItem>> GetFavoriteProductsAsync();
    Task<IEnumerable<CategoryItem>> GetLowStockProductsAsync(int threshold = 10);
    Task<CategoryItem?> GetProductByBarcodeAsync(string barcode);
    Task<IEnumerable<CategoryItem>> GetProductsOrderedAsync();
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
```

## Testing Checklist

After integration:

- [ ] Categories load without errors
- [ ] Categories display with correct names
- [ ] Products load for each category
- [ ] Product prices display correctly (from Cost column)
- [ ] Product images display (from ImagePath column)
- [ ] Search functionality works
- [ ] No null reference exceptions
- [ ] API endpoints return correct data
- [ ] Frontend displays data correctly

## Rollback Plan

If issues occur:
1. Revert PosDbContext changes
2. Revert repository changes
3. Revert service changes
4. Keep scaffolded models for future use

## Benefits of Integration

1. **Accurate Data**: All database columns are now accessible
2. **No Missing Columns**: Description, ImagePath, Cost, etc. are all available
3. **Proper Relationships**: Navigation properties work correctly
4. **Type Safety**: Correct data types prevent runtime errors
5. **Future-Proof**: Easy to add new columns as database evolves

## Next Steps

1. Execute Step 1: Update PosDbContext
2. Execute Step 2: Update ProductRepository
3. Execute Step 3: Update ProductService
4. Test the changes
5. Update other components as needed
