# Database Scaffolding Guide - POS Legacy Database

## Critical Architecture Issue

**Problem**: The current entity models in `Pos.Web.Infrastructure/Entities/` were manually created and don't accurately reflect the actual database schema. This causes:
- Missing columns that exist in the database
- Incorrect data types
- Missing relationships
- Runtime errors when querying
- Categories not loading (Description column issue)

**Solution**: Scaffold the entity models directly from the existing POS database using EF Core tooling.

## Prerequisites

1. **Install EF Core Tools** (if not already installed):
```bash
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef
```

2. **Verify Connection String** in `Pos.Web.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PosDatabase": "Server=localhost;Database=POS;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Scaffolding Strategy

### Option 1: Scaffold All Tables (Recommended for Initial Setup)

This will generate entities for all tables in the POS database.

```bash
cd Pos.Web/Pos.Web.Infrastructure

dotnet ef dbcontext scaffold "Server=localhost;Database=POS;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Entities/Legacy --context-dir Data --context PosLegacyDbContext --force --no-onconfiguring --data-annotations
```

**Parameters Explained**:
- `--output-dir Entities/Legacy`: Put generated entities in a separate folder
- `--context-dir Data`: Put DbContext in Data folder
- `--context PosLegacyDbContext`: Name the context to avoid conflicts
- `--force`: Overwrite existing files
- `--no-onconfiguring`: Don't include connection string in generated context
- `--data-annotations`: Use data annotations instead of fluent API

### Option 2: Scaffold Specific Tables Only

If you only want specific tables:

```bash
cd Pos.Web/Pos.Web.Infrastructure

dotnet ef dbcontext scaffold "Server=localhost;Database=POS;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Entities/Legacy --context-dir Data --context PosLegacyDbContext --force --no-onconfiguring --data-annotations --table Categories --table CategoryItems --table Customers --table Invoices --table InvoiceItems --table Users
```

### Option 3: Scaffold with Schema Filter

If you want to exclude certain schemas:

```bash
cd Pos.Web/Pos.Web.Infrastructure

dotnet ef dbcontext scaffold "Server=localhost;Database=POS;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Entities/Legacy --context-dir Data --context PosLegacyDbContext --force --no-onconfiguring --data-annotations --schema dbo
```

## Post-Scaffolding Steps

### 1. Review Generated Files

After scaffolding, you'll have:
```
Pos.Web.Infrastructure/
├── Data/
│   └── PosLegacyDbContext.cs (generated DbContext)
├── Entities/
│   └── Legacy/
│       ├── Category.cs (accurate model)
│       ├── CategoryItem.cs (accurate model)
│       ├── Customer.cs (accurate model)
│       ├── Invoice.cs (accurate model)
│       ├── InvoiceItem.cs (accurate model)
│       └── ... (all other tables)
```

### 2. Compare with Current Models

Check what columns were missing:

```bash
# Compare old Category.cs with new one
# Old: Pos.Web.Infrastructure/Entities/Category.cs
# New: Pos.Web.Infrastructure/Entities/Legacy/Category.cs
```

### 3. Update PosDbContext Configuration

Modify `PosDbContext.cs` to use the scaffolded models:

```csharp
using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Entities.Legacy; // Use scaffolded entities

namespace Pos.Web.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    // Legacy dbo schema tables (from scaffolded models)
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<CategoryItem> CategoryItems { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    
    // Web schema tables (keep existing)
    public DbSet<OrderLock> OrderLocks { get; set; } = null!;
    public DbSet<ApiAuditLog> ApiAuditLogs { get; set; } = null!;
    public DbSet<FeatureFlag> FeatureFlags { get; set; } = null!;
    public DbSet<SyncQueue> SyncQueues { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Import configuration from scaffolded context
        // Copy the OnModelCreating content from PosLegacyDbContext.cs
        
        // Then add web schema configurations
        ConfigureOrderLock(modelBuilder);
        ConfigureApiAuditLog(modelBuilder);
        ConfigureFeatureFlag(modelBuilder);
        ConfigureSyncQueue(modelBuilder);
    }
    
    // Keep existing web schema configuration methods
    private void ConfigureOrderLock(ModelBuilder modelBuilder) { /* existing code */ }
    private void ConfigureApiAuditLog(ModelBuilder modelBuilder) { /* existing code */ }
    private void ConfigureFeatureFlag(ModelBuilder modelBuilder) { /* existing code */ }
    private void ConfigureSyncQueue(ModelBuilder modelBuilder) { /* existing code */ }
}
```

### 4. Create Adapter Layer (Recommended)

Instead of directly using scaffolded entities everywhere, create an adapter layer:

```
Pos.Web.Infrastructure/
├── Entities/
│   ├── Legacy/          (scaffolded - don't modify)
│   │   ├── Category.cs
│   │   └── CategoryItem.cs
│   └── Adapters/        (your clean models)
│       ├── Product.cs   (adapts CategoryItem)
│       └── Order.cs     (adapts Invoice)
```

Example adapter:

```csharp
// Pos.Web.Infrastructure/Entities/Adapters/Product.cs
namespace Pos.Web.Infrastructure.Entities.Adapters;

/// <summary>
/// Clean model that adapts the legacy CategoryItem entity
/// </summary>
public class Product
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    
    // Navigation
    public Category? Category { get; set; }
    
    // Factory method to create from legacy entity
    public static Product FromLegacy(Legacy.CategoryItem item)
    {
        return new Product
        {
            ID = item.ID,
            Name = item.Name,
            CategoryID = item.CategoryID,
            Price = item.Price ?? 0, // Handle nullable
            IsAvailable = item.IsActive
        };
    }
}
```

### 5. Update Repositories

Update repositories to use scaffolded entities:

```csharp
public class ProductRepository : GenericRepository<Legacy.CategoryItem>, IProductRepository
{
    public ProductRepository(PosDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Legacy.CategoryItem>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryID == categoryId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }
}
```

### 6. Update Services

Update services to map between legacy entities and DTOs:

```csharp
public class ProductService : IProductService
{
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _unitOfWork.Products.GetCategoriesAsync();
        
        return categories.Select(c => new CategoryDto
        {
            Id = c.ID,
            Name = c.Name,
            Description = c.Description, // Now this column exists!
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive
        }).ToList();
    }
}
```

## Handling Naming Conventions

The scaffolded entities will use database column names exactly. You may want to:

### Option A: Keep Database Names
Use the exact database column names in your code.

### Option B: Use Custom Mapping
Map database columns to cleaner property names:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<CategoryItem>(entity =>
    {
        entity.ToTable("CategoryItems", "dbo");
        
        // Map database columns to cleaner property names
        entity.Property(e => e.ID).HasColumnName("ID");
        entity.Property(e => e.Name).HasColumnName("Name");
        entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
        // etc.
    });
}
```

## Migration Path

### Phase 1: Scaffold and Verify (Immediate)
1. Run scaffolding command
2. Review generated entities
3. Compare with current entities
4. Document missing columns

### Phase 2: Create Adapter Layer (Week 1)
1. Create `Entities/Adapters/` folder
2. Create clean adapter models
3. Add factory methods for conversion
4. Update one repository as proof of concept

### Phase 3: Update Repositories (Week 2)
1. Update all repositories to use scaffolded entities
2. Update repository interfaces if needed
3. Test each repository

### Phase 4: Update Services (Week 3)
1. Update services to use adapters
2. Update DTOs if needed
3. Test all service methods

### Phase 5: Cleanup (Week 4)
1. Remove old manual entity files
2. Update documentation
3. Final testing

## Common Issues and Solutions

### Issue 1: Circular References

**Problem**: Scaffolded entities have navigation properties that cause circular references.

**Solution**: Use `[JsonIgnore]` or configure JSON serialization:

```csharp
// In Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
```

### Issue 2: Nullable Reference Types

**Problem**: Scaffolded entities may have nullable warnings.

**Solution**: Either:
- Disable nullable warnings for Legacy folder
- Or add `#nullable disable` to generated files

```csharp
// At top of each generated file
#nullable disable

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Category
{
    // ...
}
```

### Issue 3: Many-to-Many Relationships

**Problem**: EF Core scaffolds join tables as entities.

**Solution**: Configure many-to-many in `OnModelCreating`:

```csharp
modelBuilder.Entity<CategoryItem>()
    .HasMany(c => c.Extras)
    .WithMany(e => e.CategoryItems)
    .UsingEntity(j => j.ToTable("CategoryItemExtras"));
```

### Issue 4: Computed Columns

**Problem**: Database has computed columns.

**Solution**: Mark them as computed:

```csharp
entity.Property(e => e.TotalWithVAT)
    .HasComputedColumnSql("[TotalCost] * (1 + [VATRate])");
```

## Verification Checklist

After scaffolding and updating:

- [ ] All tables are scaffolded correctly
- [ ] Categories table has Description column
- [ ] CategoryItems table has all price-related columns
- [ ] Invoices table has all required columns
- [ ] Navigation properties are configured
- [ ] Repositories compile without errors
- [ ] Services compile without errors
- [ ] API endpoints return correct data
- [ ] Frontend displays categories correctly
- [ ] No runtime errors when querying database

## Example: Categories Fix

**Before (Manual Model)**:
```csharp
public class Category
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    // Missing: Description column!
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
```

**After (Scaffolded Model)**:
```csharp
public partial class Category
{
    public int ID { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; } // Now included!
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();
}
```

## Next Steps

1. **Run the scaffolding command** to generate accurate models
2. **Review the generated files** to see what was missing
3. **Create a migration plan** for updating your code
4. **Test thoroughly** before deploying

This approach ensures your entity models accurately reflect the database schema and prevents issues like the Categories Description column problem.
