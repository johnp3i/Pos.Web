# Categories Mapping Fix - Action Plan

## Issue
Categories are not showing in the UI even though the Categories table exists in the legacy database (PosDbForAll project).

## Root Cause Analysis
The Categories table exists in the legacy database, but our EF Core mapping might not match the actual table schema from the Entity Framework 6 EDMX model.

## Investigation Steps

### 1. Check Actual Table Schema
Run this query to see the actual Categories table structure:

```sql
-- Run: Pos.Web/query-categories-schema.sql
sqlcmd -S localhost -d POS -i Pos.Web/query-categories-schema.sql
```

### 2. Compare with Our Entity
Our `Category` entity expects:
- `ID` (int)
- `Name` (string)
- `Description` (string, nullable)
- `DisplayOrder` (int)
- `IsActive` (bool)

### 3. Check API Response
Test the categories endpoint directly:

```bash
curl http://localhost:5001/api/products/categories
```

Expected response if working:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Category Name",
      "description": "...",
      "displayOrder": 1,
      "isActive": true
    }
  ]
}
```

Expected response if failing:
```json
{
  "success": true,
  "data": [],
  "message": null
}
```

### 4. Check API Logs
Look for errors in the API console when the categories endpoint is called.

## Possible Issues

### Issue 1: Column Name Mismatch
The legacy EDMX might use different column names than our EF Core mapping expects.

**Solution**: Update `PosDbContext.ConfigureCategory()` to map to actual column names.

### Issue 2: IsActive Column Doesn't Exist
The legacy table might not have an `IsActive` column.

**Solution**: Remove the `IsActive` filter from the query (already done).

### Issue 3: Table Name or Schema Mismatch
The table might be in a different schema or have a different name.

**Solution**: Verify table name and schema in the query script.

## Quick Fix Steps

### Step 1: Test the API Endpoint
```bash
# Start the API if not running
cd Pos.Web/Pos.Web.API
dotnet run --urls="http://localhost:5001"

# In another terminal, test the endpoint
curl http://localhost:5001/api/products/categories
```

### Step 2: Check API Logs
Look for any errors or warnings when the endpoint is called.

### Step 3: Run the Schema Query
```bash
sqlcmd -S localhost -d POS -i Pos.Web/query-categories-schema.sql
```

This will show:
- Whether the Categories table exists
- The actual column names and types
- Sample data from the table
- Whether CategoryItems has a CategoryID column

### Step 4: Update Mapping if Needed
Based on the query results, update the entity mapping in `PosDbContext.cs`.

## Next Steps

1. **Run the schema query** to see the actual table structure
2. **Test the API endpoint** to see what error (if any) is returned
3. **Check the API logs** for detailed error messages
4. **Update the entity mapping** based on findings
5. **Restart the API** and test again

## Files to Check
- `Pos.Web/Pos.Web.Infrastructure/Data/PosDbContext.cs` - Entity mapping
- `Pos.Web/Pos.Web.Infrastructure/Entities/Category.cs` - Entity definition
- `Pos.Web/Pos.Web.Infrastructure/Repositories/ProductRepository.cs` - Query logic
- `Pos.Web/query-categories-schema.sql` - Schema inspection script

## Expected Outcome
After fixing the mapping, the categories endpoint should return data, and the UI should display category chips.
