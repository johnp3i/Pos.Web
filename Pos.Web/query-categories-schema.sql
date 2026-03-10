-- Query to check Categories table schema
USE [POS]
GO

-- Check if Categories table exists
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
BEGIN
    PRINT 'Categories table exists';
    
    -- Get table schema
    SELECT 
        c.COLUMN_NAME,
        c.DATA_TYPE,
        c.CHARACTER_MAXIMUM_LENGTH,
        c.IS_NULLABLE,
        c.COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS c
    WHERE c.TABLE_SCHEMA = 'dbo' 
    AND c.TABLE_NAME = 'Categories'
    ORDER BY c.ORDINAL_POSITION;
    
    -- Get sample data
    SELECT TOP 10 * FROM dbo.Categories;
    
    -- Check if CategoryItems has CategoryID
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CategoryItems') AND name = 'CategoryID')
    BEGIN
        PRINT 'CategoryItems.CategoryID column exists';
        
        -- Check products with categories
        SELECT 
            c.ID as CategoryID,
            c.Name as CategoryName,
            COUNT(ci.ID) as ProductCount
        FROM dbo.Categories c
        LEFT JOIN dbo.CategoryItems ci ON c.ID = ci.CategoryID
        GROUP BY c.ID, c.Name
        ORDER BY c.ID;
    END
    ELSE
    BEGIN
        PRINT 'CategoryItems.CategoryID column does NOT exist';
    END
END
ELSE
BEGIN
    PRINT 'Categories table does NOT exist';
END
GO
