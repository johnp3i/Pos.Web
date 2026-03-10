-- =============================================
-- Web-Based POS System - Database Schema Setup
-- =============================================
-- This script creates the 'web' schema and tables
-- for the web-based POS system alongside the existing
-- legacy 'dbo' schema used by the WPF POS application.
--
-- Requirements: TC-1, MR-1, NFR-4
-- =============================================

USE [POS]
GO

-- =============================================
-- 1. Create 'web' schema
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'web')
BEGIN
    EXEC('CREATE SCHEMA [web]')
    PRINT 'Schema [web] created successfully'
END
ELSE
BEGIN
    PRINT 'Schema [web] already exists'
END
GO

-- =============================================
-- 2. Create web.OrderLocks table
-- =============================================
-- Purpose: Prevent concurrent editing of orders
-- Requirements: US-1.4, US-6.2
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[OrderLocks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [web].[OrderLocks]
    (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [OrderID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [LockAcquiredAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LockExpiresAt] DATETIME2(7) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [SessionID] NVARCHAR(100) NULL,
        [DeviceInfo] NVARCHAR(200) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_OrderLocks] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_OrderLocks_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[PendingInvoices]([ID]),
        CONSTRAINT [FK_OrderLocks_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID])
    )
    
    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_OrderLocks_OrderID_IsActive] 
        ON [web].[OrderLocks]([OrderID], [IsActive])
        INCLUDE ([UserID], [LockExpiresAt])
    
    CREATE NONCLUSTERED INDEX [IX_OrderLocks_LockExpiresAt] 
        ON [web].[OrderLocks]([LockExpiresAt])
        WHERE [IsActive] = 1
    
    PRINT 'Table [web].[OrderLocks] created successfully'
END
ELSE
BEGIN
    PRINT 'Table [web].[OrderLocks] already exists'
END
GO

-- =============================================
-- 3. Create web.ApiAuditLog table
-- =============================================
-- Purpose: Track all API operations for security and debugging
-- Requirements: FR-3, NFR-4
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[ApiAuditLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [web].[ApiAuditLog]
    (
        [ID] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UserID] INT NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [EntityType] NVARCHAR(100) NULL,
        [EntityID] INT NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [IPAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [RequestPath] NVARCHAR(500) NULL,
        [RequestMethod] NVARCHAR(10) NULL,
        [StatusCode] INT NULL,
        [Duration] INT NULL, -- milliseconds
        [ErrorMessage] NVARCHAR(MAX) NULL,
        
        CONSTRAINT [PK_ApiAuditLog] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_ApiAuditLog_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID])
    )
    
    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_ApiAuditLog_Timestamp] 
        ON [web].[ApiAuditLog]([Timestamp] DESC)
    
    CREATE NONCLUSTERED INDEX [IX_ApiAuditLog_UserID_Timestamp] 
        ON [web].[ApiAuditLog]([UserID], [Timestamp] DESC)
    
    CREATE NONCLUSTERED INDEX [IX_ApiAuditLog_Action_Timestamp] 
        ON [web].[ApiAuditLog]([Action], [Timestamp] DESC)
    
    PRINT 'Table [web].[ApiAuditLog] created successfully'
END
ELSE
BEGIN
    PRINT 'Table [web].[ApiAuditLog] already exists'
END
GO

-- =============================================
-- 4. Create web.UserSessions table
-- =============================================
-- Purpose: Track active user sessions for JWT token management
-- Requirements: FR-4, NFR-1
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[UserSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [web].[UserSessions]
    (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [UserID] INT NOT NULL,
        [SessionID] NVARCHAR(100) NOT NULL,
        [RefreshToken] NVARCHAR(500) NOT NULL,
        [RefreshTokenExpiresAt] DATETIME2(7) NOT NULL,
        [DeviceInfo] NVARCHAR(200) NULL,
        [IPAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LastActivityAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LoggedOutAt] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_UserSessions] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_UserSessions_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID]),
        CONSTRAINT [UQ_UserSessions_SessionID] UNIQUE ([SessionID])
    )
    
    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_UserSessions_UserID_IsActive] 
        ON [web].[UserSessions]([UserID], [IsActive])
        INCLUDE ([SessionID], [LastActivityAt])
    
    CREATE NONCLUSTERED INDEX [IX_UserSessions_RefreshToken] 
        ON [web].[UserSessions]([RefreshToken])
        WHERE [IsActive] = 1
    
    CREATE NONCLUSTERED INDEX [IX_UserSessions_RefreshTokenExpiresAt] 
        ON [web].[UserSessions]([RefreshTokenExpiresAt])
        WHERE [IsActive] = 1
    
    PRINT 'Table [web].[UserSessions] created successfully'
END
ELSE
BEGIN
    PRINT 'Table [web].[UserSessions] already exists'
END
GO

-- =============================================
-- 5. Create web.FeatureFlags table
-- =============================================
-- Purpose: Enable/disable features dynamically without deployment
-- Requirements: US-9.2, NFR-4
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[FeatureFlags]') AND type in (N'U'))
BEGIN
    CREATE TABLE [web].[FeatureFlags]
    (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsEnabled] BIT NOT NULL DEFAULT 0,
        [EnabledForUserIDs] NVARCHAR(MAX) NULL, -- JSON array of user IDs
        [EnabledForRoles] NVARCHAR(MAX) NULL, -- JSON array of role names
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] INT NULL,
        
        CONSTRAINT [PK_FeatureFlags] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [UQ_FeatureFlags_Name] UNIQUE ([Name]),
        CONSTRAINT [FK_FeatureFlags_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Users]([ID])
    )
    
    -- Index for performance
    CREATE NONCLUSTERED INDEX [IX_FeatureFlags_IsEnabled] 
        ON [web].[FeatureFlags]([IsEnabled])
        INCLUDE ([Name], [EnabledForUserIDs], [EnabledForRoles])
    
    PRINT 'Table [web].[FeatureFlags] created successfully'
END
ELSE
BEGIN
    PRINT 'Table [web].[FeatureFlags] already exists'
END
GO

-- =============================================
-- 6. Create web.SyncQueue table
-- =============================================
-- Purpose: Queue offline operations for synchronization
-- Requirements: US-7.1, US-7.2
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[SyncQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE [web].[SyncQueue]
    (
        [ID] BIGINT IDENTITY(1,1) NOT NULL,
        [UserID] INT NOT NULL,
        [DeviceID] NVARCHAR(100) NOT NULL,
        [OperationType] NVARCHAR(50) NOT NULL, -- 'Create', 'Update', 'Delete'
        [EntityType] NVARCHAR(100) NOT NULL, -- 'Order', 'Customer', etc.
        [EntityID] INT NULL,
        [Payload] NVARCHAR(MAX) NOT NULL, -- JSON payload
        [ClientTimestamp] DATETIME2(7) NOT NULL,
        [ServerTimestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Processing', 'Completed', 'Failed'
        [AttemptCount] INT NOT NULL DEFAULT 0,
        [LastAttemptAt] DATETIME2(7) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [ProcessedAt] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_SyncQueue] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_SyncQueue_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID])
    )
    
    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_SyncQueue_Status_ClientTimestamp] 
        ON [web].[SyncQueue]([Status], [ClientTimestamp] ASC)
        INCLUDE ([UserID], [DeviceID], [OperationType])
    
    CREATE NONCLUSTERED INDEX [IX_SyncQueue_UserID_DeviceID_Status] 
        ON [web].[SyncQueue]([UserID], [DeviceID], [Status])
    
    PRINT 'Table [web].[SyncQueue] created successfully'
END
ELSE
BEGIN
    PRINT 'Table [web].[SyncQueue] already exists'
END
GO

-- =============================================
-- 7. Create maintenance stored procedures
-- =============================================

-- =============================================
-- 7.1 Cleanup expired order locks
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[CleanupExpiredLocks]') AND type in (N'P'))
BEGIN
    DROP PROCEDURE [web].[CleanupExpiredLocks]
END
GO

CREATE PROCEDURE [web].[CleanupExpiredLocks]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    
    -- Deactivate expired locks
    UPDATE [web].[OrderLocks]
    SET [IsActive] = 0,
        [UpdatedAt] = GETUTCDATE()
    WHERE [IsActive] = 1
      AND [LockExpiresAt] < GETUTCDATE();
    
    SET @DeletedCount = @@ROWCOUNT;
    
    -- Delete old inactive locks (older than 7 days)
    DELETE FROM [web].[OrderLocks]
    WHERE [IsActive] = 0
      AND [UpdatedAt] < DATEADD(DAY, -7, GETUTCDATE());
    
    PRINT 'Cleanup completed: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' expired locks deactivated';
END
GO

PRINT 'Stored procedure [web].[CleanupExpiredLocks] created successfully'
GO

-- =============================================
-- 7.2 Cleanup old audit logs
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[CleanupOldAuditLogs]') AND type in (N'P'))
BEGIN
    DROP PROCEDURE [web].[CleanupOldAuditLogs]
END
GO

CREATE PROCEDURE [web].[CleanupOldAuditLogs]
    @RetentionDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    DECLARE @CutoffDate DATETIME2(7) = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    -- Delete old audit logs
    DELETE FROM [web].[ApiAuditLog]
    WHERE [Timestamp] < @CutoffDate;
    
    SET @DeletedCount = @@ROWCOUNT;
    
    PRINT 'Cleanup completed: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' audit logs deleted (older than ' + CAST(@RetentionDays AS NVARCHAR(10)) + ' days)';
END
GO

PRINT 'Stored procedure [web].[CleanupOldAuditLogs] created successfully'
GO

-- =============================================
-- 7.3 Cleanup expired sessions
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[web].[CleanupExpiredSessions]') AND type in (N'P'))
BEGIN
    DROP PROCEDURE [web].[CleanupExpiredSessions]
END
GO

CREATE PROCEDURE [web].[CleanupExpiredSessions]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    
    -- Deactivate expired sessions
    UPDATE [web].[UserSessions]
    SET [IsActive] = 0,
        [LoggedOutAt] = GETUTCDATE()
    WHERE [IsActive] = 1
      AND [RefreshTokenExpiresAt] < GETUTCDATE();
    
    SET @DeletedCount = @@ROWCOUNT;
    
    -- Delete old inactive sessions (older than 30 days)
    DELETE FROM [web].[UserSessions]
    WHERE [IsActive] = 0
      AND [LoggedOutAt] < DATEADD(DAY, -30, GETUTCDATE());
    
    PRINT 'Cleanup completed: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' expired sessions deactivated';
END
GO

PRINT 'Stored procedure [web].[CleanupExpiredSessions] created successfully'
GO

-- =============================================
-- 8. Insert default feature flags
-- =============================================
IF NOT EXISTS (SELECT * FROM [web].[FeatureFlags] WHERE [Name] = 'EnableOfflineMode')
BEGIN
    INSERT INTO [web].[FeatureFlags] ([Name], [Description], [IsEnabled])
    VALUES ('EnableOfflineMode', 'Enable offline order creation and sync', 1);
    PRINT 'Feature flag ''EnableOfflineMode'' created';
END

IF NOT EXISTS (SELECT * FROM [web].[FeatureFlags] WHERE [Name] = 'EnableKitchenDisplay')
BEGIN
    INSERT INTO [web].[FeatureFlags] ([Name], [Description], [IsEnabled])
    VALUES ('EnableKitchenDisplay', 'Enable real-time kitchen display updates', 1);
    PRINT 'Feature flag ''EnableKitchenDisplay'' created';
END

IF NOT EXISTS (SELECT * FROM [web].[FeatureFlags] WHERE [Name] = 'EnableOrderLocking')
BEGIN
    INSERT INTO [web].[FeatureFlags] ([Name], [Description], [IsEnabled])
    VALUES ('EnableOrderLocking', 'Enable order locking to prevent concurrent edits', 1);
    PRINT 'Feature flag ''EnableOrderLocking'' created';
END

IF NOT EXISTS (SELECT * FROM [web].[FeatureFlags] WHERE [Name] = 'EnableLoyaltyPoints')
BEGIN
    INSERT INTO [web].[FeatureFlags] ([Name], [Description], [IsEnabled])
    VALUES ('EnableLoyaltyPoints', 'Enable loyalty points calculation and redemption', 0);
    PRINT 'Feature flag ''EnableLoyaltyPoints'' created';
END

IF NOT EXISTS (SELECT * FROM [web].[FeatureFlags] WHERE [Name] = 'EnableBarcodeScanner')
BEGIN
    INSERT INTO [web].[FeatureFlags] ([Name], [Description], [IsEnabled])
    VALUES ('EnableBarcodeScanner', 'Enable barcode scanner support', 0);
    PRINT 'Feature flag ''EnableBarcodeScanner'' created';
END
GO

-- =============================================
-- 9. Create SQL Agent jobs for maintenance (optional)
-- =============================================
-- Note: Uncomment and configure if SQL Server Agent is available
/*
USE [msdb]
GO

-- Job: Cleanup Expired Locks (runs every 5 minutes)
EXEC dbo.sp_add_job
    @job_name = N'Web POS - Cleanup Expired Locks',
    @enabled = 1,
    @description = N'Deactivates expired order locks every 5 minutes';

EXEC dbo.sp_add_jobstep
    @job_name = N'Web POS - Cleanup Expired Locks',
    @step_name = N'Run Cleanup',
    @subsystem = N'TSQL',
    @command = N'EXEC [web].[CleanupExpiredLocks]',
    @database_name = N'POS';

EXEC dbo.sp_add_schedule
    @schedule_name = N'Every 5 Minutes',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 4,
    @freq_subday_interval = 5;

EXEC dbo.sp_attach_schedule
    @job_name = N'Web POS - Cleanup Expired Locks',
    @schedule_name = N'Every 5 Minutes';

EXEC dbo.sp_add_jobserver
    @job_name = N'Web POS - Cleanup Expired Locks';

-- Job: Cleanup Old Audit Logs (runs daily at 2 AM)
EXEC dbo.sp_add_job
    @job_name = N'Web POS - Cleanup Old Audit Logs',
    @enabled = 1,
    @description = N'Deletes audit logs older than 90 days';

EXEC dbo.sp_add_jobstep
    @job_name = N'Web POS - Cleanup Old Audit Logs',
    @step_name = N'Run Cleanup',
    @subsystem = N'TSQL',
    @command = N'EXEC [web].[CleanupOldAuditLogs] @RetentionDays = 90',
    @database_name = N'POS';

EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC dbo.sp_attach_schedule
    @job_name = N'Web POS - Cleanup Old Audit Logs',
    @schedule_name = N'Daily at 2 AM';

EXEC dbo.sp_add_jobserver
    @job_name = N'Web POS - Cleanup Old Audit Logs';

-- Job: Cleanup Expired Sessions (runs every hour)
EXEC dbo.sp_add_job
    @job_name = N'Web POS - Cleanup Expired Sessions',
    @enabled = 1,
    @description = N'Deactivates expired user sessions every hour';

EXEC dbo.sp_add_jobstep
    @job_name = N'Web POS - Cleanup Expired Sessions',
    @step_name = N'Run Cleanup',
    @subsystem = N'TSQL',
    @command = N'EXEC [web].[CleanupExpiredSessions]',
    @database_name = N'POS';

EXEC dbo.sp_add_schedule
    @schedule_name = N'Every Hour',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 8,
    @freq_subday_interval = 1;

EXEC dbo.sp_attach_schedule
    @job_name = N'Web POS - Cleanup Expired Sessions',
    @schedule_name = N'Every Hour';

EXEC dbo.sp_add_jobserver
    @job_name = N'Web POS - Cleanup Expired Sessions';

PRINT 'SQL Agent jobs created successfully';
*/

-- =============================================
-- 10. Grant permissions
-- =============================================
-- Note: Adjust user name based on your environment
-- This assumes a database user 'WebPosAppUser' exists
/*
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebPosAppUser')
BEGIN
    -- Grant SELECT on dbo schema (read-only access to legacy tables)
    GRANT SELECT ON SCHEMA::[dbo] TO [WebPosAppUser];
    
    -- Grant full access on web schema
    GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[web] TO [WebPosAppUser];
    
    -- Grant EXECUTE on stored procedures
    GRANT EXECUTE ON [web].[CleanupExpiredLocks] TO [WebPosAppUser];
    GRANT EXECUTE ON [web].[CleanupOldAuditLogs] TO [WebPosAppUser];
    GRANT EXECUTE ON [web].[CleanupExpiredSessions] TO [WebPosAppUser];
    
    PRINT 'Permissions granted to WebPosAppUser';
END
ELSE
BEGIN
    PRINT 'WARNING: Database user ''WebPosAppUser'' not found. Please create the user and grant permissions manually.';
END
*/

-- =============================================
-- Script execution complete
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Database schema setup completed successfully!';
PRINT '=============================================';
PRINT '';
PRINT 'Created:';
PRINT '  - Schema: [web]';
PRINT '  - Table: [web].[OrderLocks]';
PRINT '  - Table: [web].[ApiAuditLog]';
PRINT '  - Table: [web].[UserSessions]';
PRINT '  - Table: [web].[FeatureFlags]';
PRINT '  - Table: [web].[SyncQueue]';
PRINT '  - Stored Procedure: [web].[CleanupExpiredLocks]';
PRINT '  - Stored Procedure: [web].[CleanupOldAuditLogs]';
PRINT '  - Stored Procedure: [web].[CleanupExpiredSessions]';
PRINT '  - 5 default feature flags';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Review and execute permission grants (section 10)';
PRINT '  2. Optionally configure SQL Agent jobs (section 9)';
PRINT '  3. Update connection string in appsettings.json';
PRINT '  4. Run Entity Framework migrations';
PRINT '=============================================';
GO


-- =============================================
-- 7. Create dbo.Categories table
-- =============================================
-- Purpose: Product categorization for filtering and organization
-- Requirements: Product catalog management
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories]
    (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [UQ_Categories_Name] UNIQUE ([Name])
    );
    
    -- Create index for performance
    CREATE NONCLUSTERED INDEX [IX_Categories_DisplayOrder] 
    ON [dbo].[Categories] ([DisplayOrder] ASC, [Name] ASC)
    WHERE [IsActive] = 1;
    
    PRINT 'Table [dbo].[Categories] created successfully';
    
    -- Insert sample categories
    INSERT INTO [dbo].[Categories] ([Name], [Description], [DisplayOrder], [IsActive])
    VALUES 
        ('Coffee', 'Hot and cold coffee beverages', 1, 1),
        ('Food', 'Meals and snacks', 2, 1),
        ('Beverages', 'Non-coffee drinks', 3, 1),
        ('Desserts', 'Sweet treats and desserts', 4, 1);
    
    PRINT 'Sample categories inserted successfully';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[Categories] already exists';
END
GO

-- =============================================
-- 8. Add CategoryID to Products table if needed
-- =============================================
-- Purpose: Link products to categories
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CategoryItems') AND name = 'CategoryID')
BEGIN
    ALTER TABLE [dbo].[CategoryItems]
    ADD [CategoryID] INT NULL;
    
    PRINT 'Column [CategoryID] added to [dbo].[CategoryItems]';
    
    -- Add foreign key constraint
    ALTER TABLE [dbo].[CategoryItems]
    ADD CONSTRAINT [FK_CategoryItems_Categories] 
    FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories]([ID]);
    
    -- Create index for performance
    CREATE NONCLUSTERED INDEX [IX_CategoryItems_CategoryID] 
    ON [dbo].[CategoryItems] ([CategoryID] ASC);
    
    PRINT 'Foreign key and index created for [CategoryID]';
    
    -- Update existing products to assign them to default category (Coffee)
    UPDATE [dbo].[CategoryItems]
    SET [CategoryID] = 1
    WHERE [CategoryID] IS NULL AND [IsAvailable] = 1;
    
    PRINT 'Existing products assigned to default category';
END
ELSE
BEGIN
    PRINT 'Column [CategoryID] already exists in [dbo].[CategoryItems]';
END
GO
