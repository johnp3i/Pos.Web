-- Test query to check Categories table
USE [POS]
GO

-- Check if table exists
SELECT 
    OBJECT_ID('dbo.Categories', 'U') as TableObjectID,
    CASE WHEN OBJECT_ID('dbo.Categories', 'U') IS NOT NULL THEN 'EXISTS' ELSE 'NOT EXISTS' END as TableStatus
GO

-- Get all categories
SELECT * FROM dbo.Categories
ORDER BY DisplayOrder, Name
GO

-- Count categories
SELECT COUNT(*) as TotalCategories FROM dbo.Categories
GO

-- Check CategoryItems relationship
SELECT 
    c.ID as CategoryID,
    c.Name as CategoryName,
    c.DisplayOrder,
    c.IsActive,
    COUNT(ci.ID) as ProductCount
FROM dbo.Categories c
LEFT JOIN dbo.CategoryItems ci ON c.ID = ci.CategoryID
GROUP BY c.ID, c.Name, c.DisplayOrder, c.IsActive
ORDER BY c.DisplayOrder, c.Name
GO
