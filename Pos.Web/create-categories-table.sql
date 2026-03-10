-- Create Categories table for product categorization
-- This table is required for the web POS system

USE [POS]
GO

-- Check if table exists, drop if it does (for clean recreation)
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;
GO

-- Create Categories table
CREATE TABLE [dbo].[Categories] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_Categories_Name] UNIQUE ([Name])
);
GO

-- Create index for performance
CREATE NONCLUSTERED INDEX [IX_Categories_DisplayOrder] 
ON [dbo].[Categories] ([DisplayOrder] ASC, [Name] ASC)
WHERE [IsActive] = 1;
GO

-- Insert sample categories for testing
INSERT INTO [dbo].[Categories] ([Name], [Description], [DisplayOrder], [IsActive])
VALUES 
    ('Coffee', 'Hot and cold coffee beverages', 1, 1),
    ('Food', 'Meals and snacks', 2, 1),
    ('Beverages', 'Non-coffee drinks', 3, 1),
    ('Desserts', 'Sweet treats and desserts', 4, 1);
GO

-- Update existing Products table to add CategoryID if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Products') AND name = 'CategoryID')
BEGIN
    ALTER TABLE [dbo].[Products]
    ADD [CategoryID] INT NULL;
    
    -- Add foreign key constraint
    ALTER TABLE [dbo].[Products]
    ADD CONSTRAINT [FK_Products_Categories] 
    FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories]([ID]);
    
    -- Create index for performance
    CREATE NONCLUSTERED INDEX [IX_Products_CategoryID] 
    ON [dbo].[Products] ([CategoryID] ASC);
END
GO

-- Update existing products to assign them to default category (Coffee)
UPDATE [dbo].[Products]
SET [CategoryID] = 1
WHERE [CategoryID] IS NULL AND [IsAvailable] = 1;
GO

PRINT 'Categories table created successfully with sample data';
GO
