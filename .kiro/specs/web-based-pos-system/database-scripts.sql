-- =============================================
-- MyChair Web-Based POS System
-- Database Schema and Table Creation Scripts
-- =============================================
-- Description: Creates the 'web' schema and all required tables
--              for the new web-based POS system
-- Author: MyChair Development Team
-- Date: 2026-02-26
-- Version: 1.0
-- =============================================

USE [YourDatabaseName] -- Replace with your actual database name
GO

-- =============================================
-- SECTION 1: SCHEMA CREATION
-- =============================================

PRINT '========================================';
PRINT 'Creating web schema...';
PRINT '========================================';
GO

-- Create web schema for new web POS tables
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'web')
BEGIN
    EXEC('CREATE SCHEMA web AUTHORIZATION dbo');
    PRINT 'Schema [web] created successfully';
END
ELSE
BEGIN
    PRINT 'Schema [web] already exists';
END
GO

-- =============================================
-- SECTION 2: TABLE CREATION
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Creating tables in web schema...';
PRINT '========================================';
GO

-- =============================================
-- Table: web.OrderLocks
-- Description: Manages concurrent editing locks to prevent
--              conflicts when multiple users edit the same order
-- =============================================

PRINT 'Creating table: web.OrderLocks';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.OrderLocks') AND type = 'U')
BEGIN
    CREATE TABLE web.OrderLocks (
        ID INT PRIMARY KEY IDENTITY(1,1),
        OrderID INT NOT NULL,
        OrderType VARCHAR(20) NOT NULL, -- 'Invoice' or 'PendingInvoice'
        LockedBy INT NOT NULL, -- UserID from dbo.Users
        LockedByUserName NVARCHAR(100) NOT NULL,
        LockedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        SessionID VARCHAR(100) NOT NULL,
        DeviceInfo NVARCHAR(200) NULL,
        CONSTRAINT FK_OrderLocks_User FOREIGN KEY (LockedBy) 
            REFERENCES dbo.Users(ID) ON DELETE CASCADE,
        CONSTRAINT CHK_OrderLocks_OrderType CHECK (OrderType IN ('Invoice', 'PendingInvoice'))
    );

    PRINT '  - Table created successfully';
END
ELSE
BEGIN
    PRINT '  - Table already exists';
END
GO

-- Create indexes for web.OrderLocks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderLocks_OrderID' AND object_id = OBJECT_ID('web.OrderLocks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderLocks_OrderID 
        ON web.OrderLocks(OrderID, OrderType) 
        INCLUDE (LockedBy, ExpiresAt);
    PRINT '  - Index IX_OrderLocks_OrderID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderLocks_ExpiresAt' AND object_id = OBJECT_ID('web.OrderLocks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderLocks_ExpiresAt 
        ON web.OrderLocks(ExpiresAt) 
        WHERE ExpiresAt > GETUTCDATE();
    PRINT '  - Index IX_OrderLocks_ExpiresAt created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderLocks_SessionID' AND object_id = OBJECT_ID('web.OrderLocks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderLocks_SessionID 
        ON web.OrderLocks(SessionID);
    PRINT '  - Index IX_OrderLocks_SessionID created';
END
GO

-- =============================================
-- Table: web.ApiAuditLog
-- Description: Tracks all API operations for security,
--              compliance, and debugging
-- =============================================

PRINT '';
PRINT 'Creating table: web.ApiAuditLog';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.ApiAuditLog') AND type = 'U')
BEGIN
    CREATE TABLE web.ApiAuditLog (
        ID BIGINT PRIMARY KEY IDENTITY(1,1),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UserID INT NULL,
        UserName NVARCHAR(100) NULL,
        Action VARCHAR(100) NOT NULL, -- 'Create', 'Update', 'Delete', 'Read'
        EntityType VARCHAR(50) NOT NULL, -- 'Order', 'Customer', 'Product', etc.
        EntityID INT NULL,
        Changes NVARCHAR(MAX) NULL, -- JSON format: {"field": {"old": "value", "new": "value"}}
        IPAddress VARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        RequestPath NVARCHAR(500) NULL,
        HttpMethod VARCHAR(10) NULL, -- 'GET', 'POST', 'PUT', 'DELETE'
        StatusCode INT NULL, -- HTTP status code
        Duration INT NULL, -- Request duration in milliseconds
        ErrorMessage NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ApiAuditLog_User FOREIGN KEY (UserID) 
            REFERENCES dbo.Users(ID) ON DELETE SET NULL
    );

    PRINT '  - Table created successfully';
END
ELSE
BEGIN
    PRINT '  - Table already exists';
END
GO

-- Create indexes for web.ApiAuditLog
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ApiAuditLog_Timestamp' AND object_id = OBJECT_ID('web.ApiAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ApiAuditLog_Timestamp 
        ON web.ApiAuditLog(Timestamp DESC);
    PRINT '  - Index IX_ApiAuditLog_Timestamp created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ApiAuditLog_UserID' AND object_id = OBJECT_ID('web.ApiAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ApiAuditLog_UserID 
        ON web.ApiAuditLog(UserID) 
        INCLUDE (Timestamp, Action, EntityType);
    PRINT '  - Index IX_ApiAuditLog_UserID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ApiAuditLog_EntityType' AND object_id = OBJECT_ID('web.ApiAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ApiAuditLog_EntityType 
        ON web.ApiAuditLog(EntityType, EntityID) 
        INCLUDE (Timestamp, Action, UserID);
    PRINT '  - Index IX_ApiAuditLog_EntityType created';
END
GO

-- =============================================
-- Table: web.UserSessions
-- Description: Manages active user sessions for the web application
-- =============================================

PRINT '';
PRINT 'Creating table: web.UserSessions';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.UserSessions') AND type = 'U')
BEGIN
    CREATE TABLE web.UserSessions (
        ID INT PRIMARY KEY IDENTITY(1,1),
        SessionID VARCHAR(100) NOT NULL UNIQUE,
        UserID INT NOT NULL,
        UserName NVARCHAR(100) NOT NULL,
        DeviceType VARCHAR(50) NOT NULL, -- 'Desktop', 'Tablet', 'Mobile'
        DeviceName NVARCHAR(200) NULL,
        BrowserName VARCHAR(50) NULL,
        BrowserVersion VARCHAR(20) NULL,
        IPAddress VARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastActivityAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        RefreshToken NVARCHAR(500) NULL,
        CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserID) 
            REFERENCES dbo.Users(ID) ON DELETE CASCADE,
        CONSTRAINT CHK_UserSessions_DeviceType CHECK (DeviceType IN ('Desktop', 'Tablet', 'Mobile'))
    );

    PRINT '  - Table created successfully';
END
ELSE
BEGIN
    PRINT '  - Table already exists';
END
GO

-- Create indexes for web.UserSessions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_SessionID' AND object_id = OBJECT_ID('web.UserSessions'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_UserSessions_SessionID 
        ON web.UserSessions(SessionID);
    PRINT '  - Index IX_UserSessions_SessionID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_UserID' AND object_id = OBJECT_ID('web.UserSessions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserSessions_UserID 
        ON web.UserSessions(UserID) 
        INCLUDE (IsActive, ExpiresAt);
    PRINT '  - Index IX_UserSessions_UserID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_ExpiresAt' AND object_id = OBJECT_ID('web.UserSessions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserSessions_ExpiresAt 
        ON web.UserSessions(ExpiresAt) 
        WHERE IsActive = 1;
    PRINT '  - Index IX_UserSessions_ExpiresAt created';
END
GO

-- =============================================
-- Table: web.FeatureFlags
-- Description: Manages feature toggles for gradual rollout and A/B testing
-- =============================================

PRINT '';
PRINT 'Creating table: web.FeatureFlags';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.FeatureFlags') AND type = 'U')
BEGIN
    CREATE TABLE web.FeatureFlags (
        ID INT PRIMARY KEY IDENTITY(1,1),
        FeatureName VARCHAR(100) NOT NULL UNIQUE,
        DisplayName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        EnabledForUserIDs NVARCHAR(MAX) NULL, -- JSON array of user IDs
        EnabledForRoles NVARCHAR(MAX) NULL, -- JSON array of role names
        EnabledPercentage INT NOT NULL DEFAULT 0, -- 0-100, for gradual rollout
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_FeatureFlags_CreatedBy FOREIGN KEY (CreatedBy) 
            REFERENCES dbo.Users(ID) ON DELETE SET NULL,
        CONSTRAINT FK_FeatureFlags_UpdatedBy FOREIGN KEY (UpdatedBy) 
            REFERENCES dbo.Users(ID) ON DELETE NO ACTION,
        CONSTRAINT CHK_FeatureFlags_Percentage CHECK (EnabledPercentage BETWEEN 0 AND 100)
    );

    PRINT '  - Table created successfully';
END
ELSE
BEGIN
    PRINT '  - Table already exists';
END
GO

-- Create indexes for web.FeatureFlags
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FeatureFlags_IsEnabled' AND object_id = OBJECT_ID('web.FeatureFlags'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FeatureFlags_IsEnabled 
        ON web.FeatureFlags(IsEnabled) 
        INCLUDE (FeatureName, EnabledPercentage);
    PRINT '  - Index IX_FeatureFlags_IsEnabled created';
END
GO

-- =============================================
-- Table: web.SyncQueue
-- Description: Manages offline sync queue for PWA offline support
-- =============================================

PRINT '';
PRINT 'Creating table: web.SyncQueue';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.SyncQueue') AND type = 'U')
BEGIN
    CREATE TABLE web.SyncQueue (
        ID BIGINT PRIMARY KEY IDENTITY(1,1),
        QueuedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UserID INT NOT NULL,
        SessionID VARCHAR(100) NOT NULL,
        EntityType VARCHAR(50) NOT NULL, -- 'Order', 'Customer', etc.
        Operation VARCHAR(20) NOT NULL, -- 'Create', 'Update', 'Delete'
        EntityData NVARCHAR(MAX) NOT NULL, -- JSON payload
        Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Processing', 'Completed', 'Failed'
        RetryCount INT NOT NULL DEFAULT 0,
        MaxRetries INT NOT NULL DEFAULT 3,
        LastError NVARCHAR(MAX) NULL,
        ProcessedAt DATETIME2 NULL,
        CONSTRAINT FK_SyncQueue_User FOREIGN KEY (UserID) 
            REFERENCES dbo.Users(ID) ON DELETE CASCADE,
        CONSTRAINT CHK_SyncQueue_Status CHECK (Status IN ('Pending', 'Processing', 'Completed', 'Failed')),
        CONSTRAINT CHK_SyncQueue_Operation CHECK (Operation IN ('Create', 'Update', 'Delete'))
    );

    PRINT '  - Table created successfully';
END
ELSE
BEGIN
    PRINT '  - Table already exists';
END
GO

-- Create indexes for web.SyncQueue
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncQueue_Status' AND object_id = OBJECT_ID('web.SyncQueue'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SyncQueue_Status 
        ON web.SyncQueue(Status, QueuedAt) 
        WHERE Status IN ('Pending', 'Processing');
    PRINT '  - Index IX_SyncQueue_Status created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncQueue_UserID' AND object_id = OBJECT_ID('web.SyncQueue'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SyncQueue_UserID 
        ON web.SyncQueue(UserID) 
        INCLUDE (Status, QueuedAt);
    PRINT '  - Index IX_SyncQueue_UserID created';
END
GO

-- =============================================
-- SECTION 3: STORED PROCEDURES
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Creating stored procedures...';
PRINT '========================================';
GO

-- =============================================
-- Stored Procedure: web.CleanupExpiredLocks
-- Description: Removes expired order locks
-- Schedule: Every 5 minutes
-- =============================================

PRINT 'Creating procedure: web.CleanupExpiredLocks';
GO

CREATE OR ALTER PROCEDURE web.CleanupExpiredLocks
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT;
    
    DELETE FROM web.OrderLocks
    WHERE ExpiresAt < GETUTCDATE();
    
    SET @DeletedCount = @@ROWCOUNT;
    
    -- Log the cleanup
    IF @DeletedCount > 0
    BEGIN
        PRINT CONCAT('Cleaned up ', @DeletedCount, ' expired lock(s) at ', GETUTCDATE());
    END
    
    SELECT @DeletedCount AS DeletedLocks;
END
GO

PRINT '  - Procedure created successfully';
GO

-- =============================================
-- Stored Procedure: web.CleanupOldAuditLogs
-- Description: Archives and removes old audit logs
-- Schedule: Daily
-- =============================================

PRINT 'Creating procedure: web.CleanupOldAuditLogs';
GO

CREATE OR ALTER PROCEDURE web.CleanupOldAuditLogs
    @RetentionDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME2;
    DECLARE @DeletedCount INT;
    
    SET @CutoffDate = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    DELETE FROM web.ApiAuditLog
    WHERE Timestamp < @CutoffDate;
    
    SET @DeletedCount = @@ROWCOUNT;
    
    -- Log the cleanup
    IF @DeletedCount > 0
    BEGIN
        PRINT CONCAT('Cleaned up ', @DeletedCount, ' audit log(s) older than ', @RetentionDays, ' days at ', GETUTCDATE());
    END
    
    SELECT @DeletedCount AS DeletedRecords, @CutoffDate AS CutoffDate;
END
GO

PRINT '  - Procedure created successfully';
GO

-- =============================================
-- Stored Procedure: web.CleanupExpiredSessions
-- Description: Marks expired sessions as inactive
-- Schedule: Every hour
-- =============================================

PRINT 'Creating procedure: web.CleanupExpiredSessions';
GO

CREATE OR ALTER PROCEDURE web.CleanupExpiredSessions
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ExpiredCount INT;
    
    UPDATE web.UserSessions
    SET IsActive = 0
    WHERE ExpiresAt < GETUTCDATE() AND IsActive = 1;
    
    SET @ExpiredCount = @@ROWCOUNT;
    
    -- Log the cleanup
    IF @ExpiredCount > 0
    BEGIN
        PRINT CONCAT('Expired ', @ExpiredCount, ' session(s) at ', GETUTCDATE());
    END
    
    SELECT @ExpiredCount AS ExpiredSessions;
END
GO

PRINT '  - Procedure created successfully';
GO

-- =============================================
-- Stored Procedure: web.GetActiveLocks
-- Description: Returns all active locks with user information
-- =============================================

PRINT 'Creating procedure: web.GetActiveLocks';
GO

CREATE OR ALTER PROCEDURE web.GetActiveLocks
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ol.ID,
        ol.OrderID,
        ol.OrderType,
        ol.LockedBy,
        ol.LockedByUserName,
        ol.LockedAt,
        ol.ExpiresAt,
        ol.SessionID,
        ol.DeviceInfo,
        DATEDIFF(SECOND, GETUTCDATE(), ol.ExpiresAt) AS SecondsUntilExpiry
    FROM web.OrderLocks ol
    WHERE ol.ExpiresAt > GETUTCDATE()
    ORDER BY ol.LockedAt DESC;
END
GO

PRINT '  - Procedure created successfully';
GO

-- =============================================
-- SECTION 4: INITIAL DATA SEEDING
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Seeding initial data...';
PRINT '========================================';
GO

-- Seed Feature Flags
PRINT 'Seeding feature flags...';
GO

IF NOT EXISTS (SELECT * FROM web.FeatureFlags WHERE FeatureName = 'OfflineMode')
BEGIN
    INSERT INTO web.FeatureFlags (FeatureName, DisplayName, Description, IsEnabled, EnabledPercentage)
    VALUES ('OfflineMode', 'Offline Mode', 'Enable PWA offline functionality', 1, 100);
    PRINT '  - OfflineMode feature flag created';
END

IF NOT EXISTS (SELECT * FROM web.FeatureFlags WHERE FeatureName = 'RealtimeUpdates')
BEGIN
    INSERT INTO web.FeatureFlags (FeatureName, DisplayName, Description, IsEnabled, EnabledPercentage)
    VALUES ('RealtimeUpdates', 'Real-time Updates', 'Enable SignalR real-time updates', 1, 100);
    PRINT '  - RealtimeUpdates feature flag created';
END

IF NOT EXISTS (SELECT * FROM web.FeatureFlags WHERE FeatureName = 'OrderLocking')
BEGIN
    INSERT INTO web.FeatureFlags (FeatureName, DisplayName, Description, IsEnabled, EnabledPercentage)
    VALUES ('OrderLocking', 'Order Locking', 'Enable concurrent order editing locks', 1, 100);
    PRINT '  - OrderLocking feature flag created';
END

IF NOT EXISTS (SELECT * FROM web.FeatureFlags WHERE FeatureName = 'ApiAuditLog')
BEGIN
    INSERT INTO web.FeatureFlags (FeatureName, DisplayName, Description, IsEnabled, EnabledPercentage)
    VALUES ('ApiAuditLog', 'API Audit Logging', 'Enable comprehensive API audit logging', 1, 100);
    PRINT '  - ApiAuditLog feature flag created';
END

IF NOT EXISTS (SELECT * FROM web.FeatureFlags WHERE FeatureName = 'BarcodeScanning')
BEGIN
    INSERT INTO web.FeatureFlags (FeatureName, DisplayName, Description, IsEnabled, EnabledPercentage)
    VALUES ('BarcodeScanning', 'Barcode Scanning', 'Enable camera-based barcode scanning', 0, 0);
    PRINT '  - BarcodeScanning feature flag created';
END

GO

-- =============================================
-- SECTION 5: PERMISSIONS
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Setting up permissions...';
PRINT '========================================';
GO

-- Create application user for web POS
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebPosAppUser')
BEGIN
    CREATE USER WebPosAppUser WITHOUT LOGIN;
    PRINT 'User WebPosAppUser created';
END
ELSE
BEGIN
    PRINT 'User WebPosAppUser already exists';
END
GO

-- Grant permissions on dbo schema (read-only for most tables)
PRINT 'Granting permissions on dbo schema...';
GRANT SELECT ON SCHEMA::dbo TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.Invoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.InvoiceItems TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.PendingInvoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.PendingInvoiceItems TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.Customers TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.CustomerAddresses TO WebPosAppUser;
GRANT INSERT ON dbo.ServerCommandsHistory TO WebPosAppUser;
PRINT '  - Permissions granted on dbo schema';
GO

-- Grant full permissions on web schema
PRINT 'Granting permissions on web schema...';
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::web TO WebPosAppUser;
GRANT EXECUTE ON SCHEMA::web TO WebPosAppUser;
PRINT '  - Permissions granted on web schema';
GO

-- =============================================
-- SECTION 6: VERIFICATION
-- =============================================

PRINT '';
PRINT '========================================';
PRINT 'Verification Summary';
PRINT '========================================';
GO

-- Verify schema
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'web')
    PRINT '✓ Schema [web] exists';
ELSE
    PRINT '✗ Schema [web] NOT found';

-- Verify tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.OrderLocks') AND type = 'U')
    PRINT '✓ Table [web.OrderLocks] exists';
ELSE
    PRINT '✗ Table [web.OrderLocks] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.ApiAuditLog') AND type = 'U')
    PRINT '✓ Table [web.ApiAuditLog] exists';
ELSE
    PRINT '✗ Table [web.ApiAuditLog] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.UserSessions') AND type = 'U')
    PRINT '✓ Table [web.UserSessions] exists';
ELSE
    PRINT '✗ Table [web.UserSessions] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.FeatureFlags') AND type = 'U')
    PRINT '✓ Table [web.FeatureFlags] exists';
ELSE
    PRINT '✗ Table [web.FeatureFlags] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.SyncQueue') AND type = 'U')
    PRINT '✓ Table [web.SyncQueue] exists';
ELSE
    PRINT '✗ Table [web.SyncQueue] NOT found';

-- Verify stored procedures
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.CleanupExpiredLocks') AND type = 'P')
    PRINT '✓ Procedure [web.CleanupExpiredLocks] exists';
ELSE
    PRINT '✗ Procedure [web.CleanupExpiredLocks] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.CleanupOldAuditLogs') AND type = 'P')
    PRINT '✓ Procedure [web.CleanupOldAuditLogs] exists';
ELSE
    PRINT '✗ Procedure [web.CleanupOldAuditLogs] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.CleanupExpiredSessions') AND type = 'P')
    PRINT '✓ Procedure [web.CleanupExpiredSessions] exists';
ELSE
    PRINT '✗ Procedure [web.CleanupExpiredSessions] NOT found';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'web.GetActiveLocks') AND type = 'P')
    PRINT '✓ Procedure [web.GetActiveLocks] exists';
ELSE
    PRINT '✗ Procedure [web.GetActiveLocks] NOT found';

-- Count feature flags
DECLARE @FeatureFlagCount INT;
SELECT @FeatureFlagCount = COUNT(*) FROM web.FeatureFlags;
PRINT CONCAT('✓ Feature Flags seeded: ', @FeatureFlagCount, ' records');

GO

PRINT '';
PRINT '========================================';
PRINT 'Database setup completed successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Review the created tables and indexes';
PRINT '2. Set up SQL Server Agent jobs for maintenance procedures';
PRINT '3. Configure connection strings in the web application';
PRINT '4. Test the database connectivity';
PRINT '';
GO
