# Categories Table Fix

## Issue
Categories are not showing in the Cashier UI because the `Categories` table doesn't exist in the legacy POS database.

## Root Cause
The `ProductRepository.GetCategoriesAsync()` method has a try-catch that returns an empty list when it encounters SQL error 208 (invalid object name), which occurs when the Categories table doesn't exist:

```csharp
catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
{
    // Categories table doesn't exist in legacy database
    return new List<Category>();
}
```

## Solution
Create the Categories table in the POS database and populate it with sample data.

## Steps to Fix

### 1. Run the SQL Script
Execute the `create-categories-table.sql` script against your POS database:

```bash
# Using sqlcmd
sqlcmd -S localhost -d POS -i Pos.Web/create-categories-table.sql

# Or using SQL Server Management Studio
# Open the file and execute it against the POS database
```

### 2. What the Script Does
- Creates the `dbo.Categories` table with proper structure
- Adds indexes for performance
- Inserts 4 sample categories (Coffee, Food, Beverages, Desserts)
- Adds `CategoryID` column to Products table if it doesn't exist
- Creates foreign key relationship between Products and Categories
- Updates existing products to assign them to the default category (Coffee)

### 3. Verify the Fix
After running the script:

1. Restart the API server (if running)
2. Clear browser cache or do a hard refresh (Ctrl+Shift+R)
3. Login to the Cashier page
4. You should now see category chips below "All Categories"

## Database Schema

### Categories Table
```sql
CREATE TABLE [dbo].[Categories] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL
);
```

### Sample Data
```sql
INSERT INTO [dbo].[Categories] ([Name], [Description], [DisplayOrder], [IsActive])
VALUES 
    ('Coffee', 'Hot and cold coffee beverages', 1, 1),
    ('Food', 'Meals and snacks', 2, 1),
    ('Beverages', 'Non-coffee drinks', 3, 1),
    ('Desserts', 'Sweet treats and desserts', 4, 1);
```

## Code Flow

### 1. Client Side (Blazor)
```
Cashier.razor OnInitializedAsync()
  ↓
Dispatcher.Dispatch(LoadCategoriesAction)
  ↓
ProductCatalogEffects.HandleLoadCategoriesAction()
  ↓
ProductApiClient.GetCategoriesAsync()
  ↓
HTTP GET /api/products/categories
```

### 2. Server Side (API)
```
ProductsController.GetCategories()
  ↓
ProductService.GetCategoriesAsync()
  ↓
ProductRepository.GetCategoriesAsync()
  ↓
Query: SELECT * FROM dbo.Categories WHERE IsActive = 1
  ↓
Returns List<Category>
```

### 3. State Management (Fluxor)
```
LoadCategoriesSuccessAction dispatched
  ↓
ProductCatalogReducers.ReduceLoadCategoriesSuccessAction()
  ↓
Updates ProductCatalogState.Categories
  ↓
CategoryFilter.razor re-renders with categories
```

## Testing

### Verify Categories API
```bash
# Test the categories endpoint
curl http://localhost:5001/api/products/categories

# Expected response:
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Coffee",
      "description": "Hot and cold coffee beverages",
      "displayOrder": 1,
      "isActive": true
    },
    ...
  ],
  "message": null
}
```

### Verify in UI
1. Login to Cashier page
2. Look for category chips below "All Categories"
3. Each chip should show:
   - Category name
   - Category icon (based on name matching)
   - Product count badge (if products exist in that category)

## Future Improvements

### 1. Category Management UI
Add admin interface to:
- Create new categories
- Edit existing categories
- Reorder categories (DisplayOrder)
- Activate/deactivate categories

### 2. Product-Category Assignment
Add UI to assign products to categories:
- Bulk category assignment
- Category filter in product management
- Drag-and-drop category organization

### 3. Category Icons
Enhance icon mapping:
- Store icon name in database
- Custom icon upload
- Icon color customization

### 4. Category Analytics
Track category performance:
- Sales by category
- Popular categories
- Category trends over time

## Related Files
- `Pos.Web/create-categories-table.sql` - SQL script to create table
- `Pos.Web/Pos.Web.Infrastructure/Entities/Category.cs` - Entity definition
- `Pos.Web/Pos.Web.Infrastructure/Repositories/ProductRepository.cs` - Repository with GetCategoriesAsync
- `Pos.Web/Pos.Web.Infrastructure/Services/ProductService.cs` - Service layer
- `Pos.Web/Pos.Web.API/Controllers/ProductsController.cs` - API endpoint
- `Pos.Web/Pos.Web.Client/Components/Product/CategoryFilter.razor` - UI component
- `Pos.Web/Pos.Web.Client/Store/ProductCatalog/*` - State management

## Notes
- The Categories table is now a permanent part of the POS database schema
- The try-catch in ProductRepository can be removed once all environments have the table
- Consider adding the Categories table creation to the main database-scripts.sql file
- Update database migration/setup documentation to include this table
