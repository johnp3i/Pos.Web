# Categories Not Showing - Troubleshooting Steps

## Quick Fix Checklist

### ✅ Step 1: Restart API Server
The EF Core configuration has been fixed. Restart your API server to apply the changes.

```bash
# Stop the API if running
# Then restart it
cd Pos.Web/Pos.Web.API
dotnet run
```

**Expected Output**: API should start without any EF Core configuration errors.

### ✅ Step 2: Test API Endpoint
Test the categories endpoint directly:

```bash
# Using curl
curl -X GET "https://localhost:7001/api/products/categories" -k

# Or using PowerShell
Invoke-WebRequest -Uri "https://localhost:7001/api/products/categories" -SkipCertificateCheck
```

**Expected Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Coffee",
      "description": "Hot and cold coffee beverages",
      "displayOrder": 1,
      "isActive": true
    }
  ],
  "message": null
}
```

### ✅ Step 3: Check Database for Categories
Run this query to verify categories exist:

```sql
USE [POS]
GO

SELECT * FROM dbo.Categories
ORDER BY DisplayOrder, Name
GO
```

**If empty**, insert sample data:
```sql
USE [POS]
GO

-- Check if Categories table is empty
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    PRINT 'Inserting sample categories...'
    
    INSERT INTO dbo.Categories (Name, Description, DisplayOrder, IsActive)
    VALUES 
        ('Coffee', 'Hot and cold coffee beverages', 1, 1),
        ('Tea', 'Various tea selections', 2, 1),
        ('Pastries', 'Fresh baked goods', 3, 1),
        ('Sandwiches', 'Sandwiches and wraps', 4, 1),
        ('Desserts', 'Sweet treats', 5, 1),
        ('Beverages', 'Soft drinks and juices', 6, 1);
    
    PRINT 'Sample categories inserted successfully'
END
ELSE
BEGIN
    PRINT 'Categories table already has data'
    SELECT COUNT(*) as CategoryCount FROM dbo.Categories
END
GO
```

### ✅ Step 4: Clear Browser Cache
Clear your browser cache and reload the application:

1. Open browser DevTools (F12)
2. Right-click the refresh button
3. Select "Empty Cache and Hard Reload"

Or use keyboard shortcut:
- Chrome/Edge: `Ctrl + Shift + Delete`
- Firefox: `Ctrl + Shift + Delete`

### ✅ Step 5: Check Browser Console
Open browser DevTools (F12) and check:

1. **Console Tab**: Look for any JavaScript errors
2. **Network Tab**: 
   - Filter by "categories"
   - Check if `/api/products/categories` request is made
   - Check the response status (should be 200 OK)
   - Check the response body (should contain categories array)

### ✅ Step 6: Check API Logs
Look for these log messages in your API console:

```
info: Pos.Web.API.Controllers.ProductsController[0]
      Getting all product categories
info: Pos.Web.Infrastructure.Services.ProductService[0]
      Loading categories from database
info: Pos.Web.Infrastructure.Services.ProductService[0]
      Loaded 6 categories from database
info: Pos.Web.API.Controllers.ProductsController[0]
      Retrieved 6 categories
```

## Detailed Diagnostic Steps

### Diagnostic 1: Verify EF Core Configuration

Check if the API starts without errors:

```bash
cd Pos.Web/Pos.Web.API
dotnet run
```

**Look for**:
- ✅ No errors about "IsRequired" on non-nullable types
- ✅ "Now listening on: https://localhost:7001"
- ✅ "Application started"

**If you see errors**, the EF Core configuration needs to be fixed.

### Diagnostic 2: Test Database Connection

Create a test endpoint to verify database connectivity:

```csharp
// Add to ProductsController.cs temporarily
[HttpGet("test-categories-raw")]
[AllowAnonymous]
public async Task<IActionResult> TestCategoriesRaw()
{
    try
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(new { 
            count = categories.Count, 
            categories = categories.Select(c => new { c.ID, c.Name, c.DisplayOrder, c.IsActive })
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
    }
}
```

Test it:
```bash
curl -X GET "https://localhost:7001/api/products/test-categories-raw" -k
```

### Diagnostic 3: Check Fluxor State

In browser console, check if categories are loaded into Fluxor state:

```javascript
// Open Redux DevTools
// Look for actions:
// - ProductCatalogActions.LoadCategoriesAction
// - ProductCatalogActions.LoadCategoriesSuccessAction

// Check state
// ProductCatalogState should have:
// - Categories: array of categories
// - IsLoadingCategories: false
```

### Diagnostic 4: Verify API Client

Add logging to ProductApiClient:

```csharp
// In ProductApiClient.GetCategoriesAsync()
_logger.LogInformation("Calling /api/products/categories");
var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("/api/products/categories");
_logger.LogInformation("Response: Success={Success}, DataCount={Count}", 
    response?.Success, response?.Data?.Count);
```

### Diagnostic 5: Check CORS Configuration

Verify CORS is configured in Program.cs:

```csharp
// Should have:
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7002")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// And:
app.UseCors("AllowBlazorClient");
```

## Common Issues and Solutions

### Issue 1: Empty Categories Array

**Symptom**: API returns `{ "success": true, "data": [], "message": null }`

**Solution**: Database table is empty. Insert sample categories (see Step 3).

### Issue 2: 500 Internal Server Error

**Symptom**: API returns 500 error

**Possible Causes**:
1. EF Core configuration error (fixed in this session)
2. Database connection issue
3. Missing columns in database table

**Solution**: Check API logs for detailed error message.

### Issue 3: 404 Not Found

**Symptom**: Browser shows 404 for `/api/products/categories`

**Solution**: 
1. Verify API is running on correct port (7001)
2. Check ProductsController has `[HttpGet("categories")]` attribute
3. Verify base route is `[Route("api/[controller]")]`

### Issue 4: CORS Error

**Symptom**: Browser console shows CORS error

**Solution**: Verify CORS configuration (see Diagnostic 5).

### Issue 5: Categories Not Rendering

**Symptom**: API returns categories but UI doesn't show them

**Possible Causes**:
1. Fluxor state not updating
2. Component not subscribing to state
3. CSS hiding the elements

**Solution**:
1. Check Redux DevTools for state updates
2. Verify `@inherits FluxorComponent` in CategoryFilter.razor
3. Check browser DevTools Elements tab for hidden elements

## Verification Checklist

After completing the steps above, verify:

- [ ] API starts without errors
- [ ] `/api/products/categories` returns categories array
- [ ] Database has categories data
- [ ] Browser console shows no errors
- [ ] Network tab shows successful API call
- [ ] Fluxor state contains categories
- [ ] CategoryFilter component renders category chips
- [ ] Clicking category chip filters products

## Success Indicators

You'll know it's working when:

1. **API Console** shows:
   ```
   info: Retrieved 6 categories
   ```

2. **Browser Network Tab** shows:
   ```
   GET /api/products/categories
   Status: 200 OK
   Response: { "success": true, "data": [...], "message": null }
   ```

3. **UI** shows:
   ```
   [All Categories] [Coffee] [Tea] [Pastries] [Sandwiches] [Desserts]
   ```

4. **Clicking a category** filters the products list

## Still Not Working?

If categories still don't show after following all steps:

1. **Capture full error details**:
   - API console output
   - Browser console errors
   - Network tab request/response
   - Redux DevTools state

2. **Check the diagnostic document**:
   - `.kiro/specs/web-based-pos-system/CATEGORIES-NOT-SHOWING-DIAGNOSIS.md`

3. **Verify all files are saved**:
   - PosDbContext.cs (EF Core configuration fix)
   - All other files should be unchanged

4. **Try a clean rebuild**:
   ```bash
   cd Pos.Web
   dotnet clean
   dotnet build
   ```

## Next Steps After Fix

Once categories are showing:

1. **Test category filtering**: Click different categories and verify products filter correctly
2. **Test "All Categories"**: Verify it shows all products
3. **Test with empty category**: Create a category with no products and verify it still shows
4. **Test category ordering**: Verify categories appear in DisplayOrder sequence
5. **Test inactive categories**: Set IsActive=0 and verify they don't show (if that's the desired behavior)
