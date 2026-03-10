-- =============================================
-- Web POS System - Database Maintenance & Permissions
-- Task 2.2 & 2.3: Stored Procedures and Security Configuration
-- =============================================
-- Description: This script creates maintenance stored procedures
--              and configures database permissions for the web POS system.
-- Database: POS (existing database)
-- Schema: web (created in Task 2.1)
-- =============================================

USE [POS];
GO

-- =============================================
-- SECTION 1: DATABASE MAINTENANCE STORED PROCEDURES (Task 2.2)
-- =============================================

PRINT '========================================';
PRINT 'Creating Database Maintenance Procedures';
PRINT '========================================';
GO

-- =============================================
-- Stored Procedure: web.CleanupExpiredLocks
-- Description: Removes expired order locks to prevent lock table bloat
-- Schedule: Run every 5 minutes via SQL Server Agent Job
-- =============================================
CREATE OR ALTER PROCEDURE web.CleanupExpiredLocks
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT;
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    
    BEGIN TRY
        -- Deactivate expired locks
        UPDATE web.OrderLocks
        SET IsActive = 0,
            UpdatedAt = GETUTCDATE()
        WHERE IsActive = 1
          AND LockExpiresAt < GETUTCDATE();
        
        SET @DeletedCount = @@ROWCOUNT;
        
        -- Delete old inactive locks (older than 7 days)
        DELETE FROM web.OrderLocks
        WHERE IsActive = 0
          AND UpdatedAt < DATEADD(DAY, -7, GETUTCDATE());
        
        -- Log the cleanup operation
        IF @DeletedCount > 0
        BEGIN
            PRINT CONCAT('Cleanup completed at ', FORMAT(@StartTime, 'yyyy-MM-dd HH:mm:ss'), 
                        ': Deactivated ', @DeletedCount, ' expired lock(s)');
        END
        
        -- Return result for monitoring
        SELECT 
            @DeletedCount AS DeactivatedLocks,
            @StartTime AS CleanupTime,
            DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()) AS DurationMs;
            
    END TRY
    BEGIN CATCH
        -- Log error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT CONCAT('ERROR in CleanupExpiredLocks: ', @ErrorMessage);
        
        -- Re-throw error
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Created: web.CleanupExpiredLocks';
GO

-- =============================================
-- Stored Procedure: web.CleanupOldAuditLogs
-- Description: Archives and removes old audit logs based on retention policy
-- Schedule: Run daily at 2:00 AM via SQL Server Agent Job
-- Default Retention: 90 days
-- =============================================
CREATE OR ALTER PROCEDURE web.CleanupOldAuditLogs
    @RetentionDays INT = 90,
    @BatchSize INT = 10000
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    DECLARE @TotalDeleted INT = 0;
    DECLARE @CutoffDate DATETIME2;
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    
    -- Validate parameters
    IF @RetentionDays < 30
    BEGIN
        RAISERROR('Retention period must be at least 30 days for compliance', 16, 1);
        RETURN;
    END
    
    SET @CutoffDate = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    BEGIN TRY
        -- Delete in batches to avoid long-running transactions
        WHILE 1 = 1
        BEGIN
            DELETE TOP (@BatchSize)
            FROM web.ApiAuditLog
            WHERE Timestamp < @CutoffDate;
            
            SET @DeletedCount = @@ROWCOUNT;
            SET @TotalDeleted = @TotalDeleted + @DeletedCount;
            
            -- Exit if no more rows to delete
            IF @DeletedCount < @BatchSize
                BREAK;
                
            -- Small delay between batches to reduce load
            WAITFOR DELAY '00:00:01';
        END
        
        -- Log the cleanup operation
        IF @TotalDeleted > 0
        BEGIN
            PRINT CONCAT('Audit log cleanup completed at ', FORMAT(@StartTime, 'yyyy-MM-dd HH:mm:ss'),
                        ': Deleted ', @TotalDeleted, ' record(s) older than ', 
                        FORMAT(@CutoffDate, 'yyyy-MM-dd'));
        END
        
        -- Return result for monitoring
        SELECT 
            @TotalDeleted AS DeletedRecords,
            @CutoffDate AS CutoffDate,
            @RetentionDays AS RetentionDays,
            @StartTime AS CleanupTime,
            DATEDIFF(SECOND, @StartTime, GETUTCDATE()) AS DurationSeconds;
            
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT CONCAT('ERROR in CleanupOldAuditLogs: ', @ErrorMessage);
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Created: web.CleanupOldAuditLogs';
GO

-- =============================================
-- Stored Procedure: web.CleanupExpiredSessions
-- Description: Marks expired user sessions as inactive
-- Schedule: Run every hour via SQL Server Agent Job
-- =============================================
CREATE OR ALTER PROCEDURE web.CleanupExpiredSessions
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UpdatedCount INT;
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    
    BEGIN TRY
        -- Mark expired sessions as inactive
        UPDATE web.UserSessions
        SET IsActive = 0,
            LoggedOutAt = GETUTCDATE()
        WHERE RefreshTokenExpiresAt < GETUTCDATE() 
          AND IsActive = 1;
        
        SET @UpdatedCount = @@ROWCOUNT;
        
        -- Delete old inactive sessions (older than 30 days)
        DELETE FROM web.UserSessions
        WHERE IsActive = 0
          AND LoggedOutAt < DATEADD(DAY, -30, GETUTCDATE());
        
        -- Log the cleanup operation
        IF @UpdatedCount > 0
        BEGIN
            PRINT CONCAT('Session cleanup completed at ', FORMAT(@StartTime, 'yyyy-MM-dd HH:mm:ss'),
                        ': Expired ', @UpdatedCount, ' session(s)');
        END
        
        -- Return result for monitoring
        SELECT 
            @UpdatedCount AS ExpiredSessions,
            @StartTime AS CleanupTime,
            DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()) AS DurationMs;
            
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT CONCAT('ERROR in CleanupExpiredSessions: ', @ErrorMessage);
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Created: web.CleanupExpiredSessions';
GO

-- =============================================
-- Stored Procedure: web.CleanupFailedSyncQueue
-- Description: Removes old failed sync queue items after max retries
-- Schedule: Run daily at 3:00 AM via SQL Server Agent Job
-- Default Retention: 7 days for failed items
-- =============================================
CREATE OR ALTER PROCEDURE web.CleanupFailedSyncQueue
    @RetentionDays INT = 7,
    @MaxAttempts INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT;
    DECLARE @CutoffDate DATETIME2;
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    
    SET @CutoffDate = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    BEGIN TRY
        -- Delete old failed sync items
        DELETE FROM web.SyncQueue
        WHERE Status = 'Failed'
          AND ClientTimestamp < @CutoffDate
          AND AttemptCount >= @MaxAttempts;
        
        SET @DeletedCount = @@ROWCOUNT;
        
        -- Log the cleanup operation
        IF @DeletedCount > 0
        BEGIN
            PRINT CONCAT('Sync queue cleanup completed at ', FORMAT(@StartTime, 'yyyy-MM-dd HH:mm:ss'),
                        ': Deleted ', @DeletedCount, ' failed item(s)');
        END
        
        -- Return result for monitoring
        SELECT 
            @DeletedCount AS DeletedItems,
            @CutoffDate AS CutoffDate,
            @RetentionDays AS RetentionDays,
            @MaxAttempts AS MaxAttempts,
            @StartTime AS CleanupTime,
            DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE()) AS DurationMs;
            
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        PRINT CONCAT('ERROR in CleanupFailedSyncQueue: ', @ErrorMessage);
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Created: web.CleanupFailedSyncQueue';
GO

-- =============================================
-- Stored Procedure: web.GetMaintenanceStatus
-- Description: Returns status of all web schema tables for monitoring
-- Usage: EXEC web.GetMaintenanceStatus
-- =============================================
CREATE OR ALTER PROCEDURE web.GetMaintenanceStatus
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Order Locks Status
    SELECT 
        'OrderLocks' AS TableName,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN LockExpiresAt > GETUTCDATE() AND IsActive = 1 THEN 1 ELSE 0 END) AS ActiveLocks,
        SUM(CASE WHEN LockExpiresAt <= GETUTCDATE() OR IsActive = 0 THEN 1 ELSE 0 END) AS ExpiredLocks,
        MIN(LockAcquiredAt) AS OldestLock,
        MAX(LockAcquiredAt) AS NewestLock
    FROM web.OrderLocks
    
    UNION ALL
    
    -- API Audit Log Status
    SELECT 
        'ApiAuditLog' AS TableName,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN Timestamp >= DATEADD(DAY, -1, GETUTCDATE()) THEN 1 ELSE 0 END) AS Last24Hours,
        SUM(CASE WHEN Timestamp >= DATEADD(DAY, -7, GETUTCDATE()) THEN 1 ELSE 0 END) AS Last7Days,
        MIN(Timestamp) AS OldestRecord,
        MAX(Timestamp) AS NewestRecord
    FROM web.ApiAuditLog
    
    UNION ALL
    
    -- User Sessions Status
    SELECT 
        'UserSessions' AS TableName,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveSessions,
        SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveSessions,
        MIN(CreatedAt) AS OldestSession,
        MAX(CreatedAt) AS NewestSession
    FROM web.UserSessions
    
    UNION ALL
    
    -- Feature Flags Status
    SELECT 
        'FeatureFlags' AS TableName,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) AS EnabledFlags,
        SUM(CASE WHEN IsEnabled = 0 THEN 1 ELSE 0 END) AS DisabledFlags,
        MIN(CreatedAt) AS OldestFlag,
        MAX(UpdatedAt) AS LastUpdate
    FROM web.FeatureFlags
    
    UNION ALL
    
    -- Sync Queue Status
    SELECT 
        'SyncQueue' AS TableName,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS PendingItems,
        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS FailedItems,
        MIN(ClientTimestamp) AS OldestItem,
        MAX(ClientTimestamp) AS NewestItem
    FROM web.SyncQueue;
END
GO

PRINT 'Created: web.GetMaintenanceStatus';
GO

PRINT '';
PRINT 'All maintenance stored procedures created successfully!';
PRINT '';
GO

-- =============================================
-- SECTION 2: DATABASE PERMISSIONS (Task 2.3)
-- =============================================

PRINT '========================================';
PRINT 'Configuring Database Permissions';
PRINT '========================================';
GO

-- =============================================
-- Create WebPosAppUser (Application User)
-- Description: Database user for the web POS application
-- Note: This user should be mapped to a SQL Server login
--       or use Windows Authentication
-- =============================================

-- Check if user already exists
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebPosAppUser')
BEGIN
    -- Create user without login (for development/testing)
    -- In production, map this to an actual SQL Server login:
    -- CREATE USER WebPosAppUser FOR LOGIN [YourSQLLogin];
    CREATE USER WebPosAppUser WITHOUT LOGIN;
    PRINT 'Created database user: WebPosAppUser';
END
ELSE
BEGIN
    PRINT 'Database user WebPosAppUser already exists';
END
GO

-- =============================================
-- Grant Permissions on dbo Schema (Legacy Tables)
-- Description: Read-only access to most tables,
--              write access only to specific tables
-- =============================================

PRINT 'Granting permissions on dbo schema...';
GO

-- Grant SELECT on entire dbo schema (read-only by default)
GRANT SELECT ON SCHEMA::dbo TO WebPosAppUser;
PRINT '  - Granted SELECT on dbo schema';
GO

-- Grant INSERT and UPDATE on invoice tables
GRANT INSERT, UPDATE ON dbo.Invoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.InvoiceItems TO WebPosAppUser;
PRINT '  - Granted INSERT, UPDATE on Invoices and InvoiceItems';
GO

-- Grant INSERT and UPDATE on pending invoice tables
GRANT INSERT, UPDATE ON dbo.PendingInvoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.PendingInvoiceItems TO WebPosAppUser;
PRINT '  - Granted INSERT, UPDATE on PendingInvoices and PendingInvoiceItems';
GO

-- Grant INSERT and UPDATE on customer tables
GRANT INSERT, UPDATE ON dbo.Customers TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.CustomerAddresses TO WebPosAppUser;
PRINT '  - Granted INSERT, UPDATE on Customers and CustomerAddresses';
GO

-- Grant INSERT on server commands history (write-only)
GRANT INSERT ON dbo.ServerCommandsHistory TO WebPosAppUser;
PRINT '  - Granted INSERT on ServerCommandsHistory';
GO

-- =============================================
-- Grant Permissions on web Schema (New Tables)
-- Description: Full CRUD access to all web schema tables
-- =============================================

PRINT 'Granting permissions on web schema...';
GO

-- Grant full CRUD permissions on web schema
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::web TO WebPosAppUser;
PRINT '  - Granted SELECT, INSERT, UPDATE, DELETE on web schema';
GO

-- Grant EXECUTE on all stored procedures in web schema
GRANT EXECUTE ON SCHEMA::web TO WebPosAppUser;
PRINT '  - Granted EXECUTE on web schema stored procedures';
GO

-- =============================================
-- Verify Permissions
-- =============================================

PRINT '';
PRINT 'Verifying permissions for WebPosAppUser...';
GO

-- Query to show all permissions for WebPosAppUser
SELECT 
    dp.class_desc AS PermissionLevel,
    OBJECT_SCHEMA_NAME(dp.major_id) AS SchemaName,
    OBJECT_NAME(dp.major_id) AS ObjectName,
    dp.permission_name AS Permission,
    dp.state_desc AS PermissionState
FROM sys.database_permissions dp
INNER JOIN sys.database_principals dpr ON dp.grantee_principal_id = dpr.principal_id
WHERE dpr.name = 'WebPosAppUser'
ORDER BY dp.class_desc, SchemaName, ObjectName, Permission;
GO

PRINT '';
PRINT '========================================';
PRINT 'Database Maintenance & Permissions Setup Complete!';
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Create SQL Server Agent Jobs for maintenance procedures:';
PRINT '   - web.CleanupExpiredLocks (every 5 minutes)';
PRINT '   - web.CleanupExpiredSessions (every hour)';
PRINT '   - web.CleanupOldAuditLogs (daily at 2:00 AM)';
PRINT '   - web.CleanupFailedSyncQueue (daily at 3:00 AM)';
PRINT '';
PRINT '2. In PRODUCTION, map WebPosAppUser to a SQL Server login:';
PRINT '   DROP USER WebPosAppUser;';
PRINT '   CREATE USER WebPosAppUser FOR LOGIN [YourSQLLogin];';
PRINT '   -- Then re-run the permissions section of this script';
PRINT '';
PRINT '3. Test maintenance procedures:';
PRINT '   EXEC web.CleanupExpiredLocks;';
PRINT '   EXEC web.CleanupExpiredSessions;';
PRINT '   EXEC web.GetMaintenanceStatus;';
PRINT '';
GO
