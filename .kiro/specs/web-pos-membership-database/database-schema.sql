-- =============================================
-- Web POS Membership Database - Schema Creation Script
-- =============================================
-- Description: Creates the WebPosMembership database with ASP.NET Core Identity tables
--              and custom authentication tables for JWT token management, session tracking,
--              audit logging, and password history.
-- 
-- Prerequisites: SQL Server 2016 or later
-- Execution: Run this script on your SQL Server instance with appropriate permissions
-- =============================================

USE master;
GO

-- =============================================
-- Step 1: Create Database
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WebPosMembership')
BEGIN
    CREATE DATABASE [WebPosMembership];
    PRINT 'Database WebPosMembership created successfully.';
END
ELSE
BEGIN
    PRINT 'Database WebPosMembership already exists.';
END
GO

USE [WebPosMembership];
GO

-- =============================================
-- Step 2: Create ASP.NET Core Identity Tables
-- =============================================

-- AspNetUsers Table (Extended with custom fields)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        -- Standard ASP.NET Core Identity fields
        [Id] NVARCHAR(450) NOT NULL,
        [UserName] NVARCHAR(256) NOT NULL,
        [NormalizedUserName] NVARCHAR(256) NOT NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL DEFAULT 0,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL DEFAULT 0,
        [TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
        [LockoutEnd] DATETIMEOFFSET(7) NULL,
        [LockoutEnabled] BIT NOT NULL DEFAULT 1,
        [AccessFailedCount] INT NOT NULL DEFAULT 0,
        
        -- Custom fields for POS integration
        [EmployeeId] INT NOT NULL,
        [FirstName] NVARCHAR(50) NULL,
        [LastName] NVARCHAR(50) NULL,
        [DisplayName] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastLoginAt] DATETIME2 NULL,
        [LastPasswordChangedAt] DATETIME2 NULL,
        [RequirePasswordChange] BIT NOT NULL DEFAULT 0,
        [IsTwoFactorEnabled] BIT NOT NULL DEFAULT 0,
        
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_AspNetUsers_EmployeeId] UNIQUE ([EmployeeId]),
        CONSTRAINT [UQ_AspNetUsers_NormalizedUserName] UNIQUE ([NormalizedUserName])
    );
    
    PRINT 'Table AspNetUsers created successfully.';
END
GO

-- AspNetRoles Table (Extended with custom fields)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(256) NOT NULL,
        [NormalizedName] NVARCHAR(256) NOT NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        
        -- Custom fields
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsSystemRole] BIT NOT NULL DEFAULT 0,
        
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_AspNetRoles_NormalizedName] UNIQUE ([NormalizedName])
    );
    
    PRINT 'Table AspNetRoles created successfully.';
END
GO

-- AspNetUserRoles Table (Many-to-Many relationship)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table AspNetUserRoles created successfully.';
END
GO

-- AspNetUserClaims Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table AspNetUserClaims created successfully.';
END
GO

-- AspNetUserLogins Table (External authentication providers)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [ProviderKey] NVARCHAR(450) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table AspNetUserLogins created successfully.';
END
GO

-- AspNetUserTokens Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Value] NVARCHAR(MAX) NULL,
        
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table AspNetUserTokens created successfully.';
END
GO

-- AspNetRoleClaims Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table AspNetRoleClaims created successfully.';
END
GO

-- =============================================
-- Step 3: Create Custom Authentication Tables
-- =============================================

-- RefreshTokens Table (JWT refresh token management)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Token] NVARCHAR(500) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ExpiresAt] DATETIME2 NOT NULL,
        [RevokedAt] DATETIME2 NULL,
        [RevokedReason] NVARCHAR(200) NULL,
        [DeviceInfo] NVARCHAR(200) NULL,
        [IpAddress] NVARCHAR(45) NULL,
        
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_RefreshTokens_Token] UNIQUE ([Token])
    );
    
    PRINT 'Table RefreshTokens created successfully.';
END
GO

-- UserSessions Table (Active session tracking)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserSessions] (
        [SessionId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [UserId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastActivityAt] DATETIME2 NULL,
        [EndedAt] DATETIME2 NULL,
        [DeviceType] NVARCHAR(20) NOT NULL,
        [DeviceInfo] NVARCHAR(200) NULL,
        [IpAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        
        CONSTRAINT [PK_UserSessions] PRIMARY KEY CLUSTERED ([SessionId] ASC),
        CONSTRAINT [FK_UserSessions_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CHK_UserSessions_DeviceType] CHECK ([DeviceType] IN ('Desktop', 'Tablet', 'Mobile'))
    );
    
    PRINT 'Table UserSessions created successfully.';
END
GO

-- AuthAuditLog Table (Security event logging)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuthAuditLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuthAuditLog] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NULL,
        [UserName] NVARCHAR(100) NULL,
        [EventType] NVARCHAR(50) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IpAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [Details] NVARCHAR(1000) NULL,
        [IsSuccessful] BIT NOT NULL,
        [ErrorMessage] NVARCHAR(500) NULL,
        
        CONSTRAINT [PK_AuthAuditLog] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AuthAuditLog_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE SET NULL
    );
    
    PRINT 'Table AuthAuditLog created successfully.';
END
GO

-- PasswordHistory Table (Password reuse prevention)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PasswordHistory] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ChangedBy] NVARCHAR(100) NULL,
        [ChangeReason] NVARCHAR(200) NULL,
        
        CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PasswordHistory_AspNetUsers] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    
    PRINT 'Table PasswordHistory created successfully.';
END
GO

-- =============================================
-- Step 4: Create Indexes for Performance
-- =============================================

-- AspNetUsers Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_EmployeeId' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_EmployeeId] ON [dbo].[AspNetUsers]([EmployeeId] ASC);
    PRINT 'Index IX_AspNetUsers_EmployeeId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_IsActive' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_IsActive] ON [dbo].[AspNetUsers]([IsActive] ASC);
    PRINT 'Index IX_AspNetUsers_IsActive created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_NormalizedEmail' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_NormalizedEmail] ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);
    PRINT 'Index IX_AspNetUsers_NormalizedEmail created successfully.';
END
GO

-- AspNetUserRoles Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID('dbo.AspNetUserRoles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId] ASC);
    PRINT 'Index IX_AspNetUserRoles_RoleId created successfully.';
END
GO

-- AspNetUserClaims Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID('dbo.AspNetUserClaims'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId] ASC);
    PRINT 'Index IX_AspNetUserClaims_UserId created successfully.';
END
GO

-- AspNetUserLogins Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID('dbo.AspNetUserLogins'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId] ASC);
    PRINT 'Index IX_AspNetUserLogins_UserId created successfully.';
END
GO

-- AspNetRoleClaims Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID('dbo.AspNetRoleClaims'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId] ASC);
    PRINT 'Index IX_AspNetRoleClaims_RoleId created successfully.';
END
GO

-- RefreshTokens Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId' AND object_id = OBJECT_ID('dbo.RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens]([UserId] ASC);
    PRINT 'Index IX_RefreshTokens_UserId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token' AND object_id = OBJECT_ID('dbo.RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens]([Token] ASC);
    PRINT 'Index IX_RefreshTokens_Token created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_ExpiresAt' AND object_id = OBJECT_ID('dbo.RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_ExpiresAt] ON [dbo].[RefreshTokens]([ExpiresAt] ASC);
    PRINT 'Index IX_RefreshTokens_ExpiresAt created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_RevokedAt' AND object_id = OBJECT_ID('dbo.RefreshTokens'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_RevokedAt] ON [dbo].[RefreshTokens]([RevokedAt] ASC);
    PRINT 'Index IX_RefreshTokens_RevokedAt created successfully.';
END
GO

-- UserSessions Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_UserId' AND object_id = OBJECT_ID('dbo.UserSessions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserSessions_UserId] ON [dbo].[UserSessions]([UserId] ASC);
    PRINT 'Index IX_UserSessions_UserId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_CreatedAt' AND object_id = OBJECT_ID('dbo.UserSessions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserSessions_CreatedAt] ON [dbo].[UserSessions]([CreatedAt] ASC);
    PRINT 'Index IX_UserSessions_CreatedAt created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_EndedAt' AND object_id = OBJECT_ID('dbo.UserSessions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserSessions_EndedAt] ON [dbo].[UserSessions]([EndedAt] ASC);
    PRINT 'Index IX_UserSessions_EndedAt created successfully.';
END
GO

-- AuthAuditLog Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuthAuditLog_UserId' AND object_id = OBJECT_ID('dbo.AuthAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuthAuditLog_UserId] ON [dbo].[AuthAuditLog]([UserId] ASC);
    PRINT 'Index IX_AuthAuditLog_UserId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuthAuditLog_EventType' AND object_id = OBJECT_ID('dbo.AuthAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuthAuditLog_EventType] ON [dbo].[AuthAuditLog]([EventType] ASC);
    PRINT 'Index IX_AuthAuditLog_EventType created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuthAuditLog_Timestamp' AND object_id = OBJECT_ID('dbo.AuthAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuthAuditLog_Timestamp] ON [dbo].[AuthAuditLog]([Timestamp] ASC);
    PRINT 'Index IX_AuthAuditLog_Timestamp created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuthAuditLog_IsSuccessful' AND object_id = OBJECT_ID('dbo.AuthAuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuthAuditLog_IsSuccessful] ON [dbo].[AuthAuditLog]([IsSuccessful] ASC);
    PRINT 'Index IX_AuthAuditLog_IsSuccessful created successfully.';
END
GO

-- PasswordHistory Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PasswordHistory_UserId' AND object_id = OBJECT_ID('dbo.PasswordHistory'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PasswordHistory_UserId] ON [dbo].[PasswordHistory]([UserId] ASC);
    PRINT 'Index IX_PasswordHistory_UserId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PasswordHistory_CreatedAt' AND object_id = OBJECT_ID('dbo.PasswordHistory'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PasswordHistory_CreatedAt] ON [dbo].[PasswordHistory]([CreatedAt] ASC);
    PRINT 'Index IX_PasswordHistory_CreatedAt created successfully.';
END
GO

-- =============================================
-- Step 5: Seed Initial Roles
-- =============================================

-- Insert system roles if they don't exist
IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'ADMIN')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [IsSystemRole], [CreatedAt])
    VALUES (NEWID(), 'Admin', 'ADMIN', 'System administrator with full access', 1, GETUTCDATE());
    PRINT 'Role Admin created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'MANAGER')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [IsSystemRole], [CreatedAt])
    VALUES (NEWID(), 'Manager', 'MANAGER', 'Store manager with elevated privileges', 1, GETUTCDATE());
    PRINT 'Role Manager created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'CASHIER')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [IsSystemRole], [CreatedAt])
    VALUES (NEWID(), 'Cashier', 'CASHIER', 'Cashier with POS access', 1, GETUTCDATE());
    PRINT 'Role Cashier created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'WAITER')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [IsSystemRole], [CreatedAt])
    VALUES (NEWID(), 'Waiter', 'WAITER', 'Waiter with order management access', 1, GETUTCDATE());
    PRINT 'Role Waiter created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'KITCHEN')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [IsSystemRole], [CreatedAt])
    VALUES (NEWID(), 'Kitchen', 'KITCHEN', 'Kitchen staff with order preparation access', 1, GETUTCDATE());
    PRINT 'Role Kitchen created successfully.';
END
GO

-- =============================================
-- Step 6: Verification
-- =============================================

PRINT '';
PRINT '=============================================';
PRINT 'Database Schema Creation Complete';
PRINT '=============================================';
PRINT '';
PRINT 'Tables Created:';
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Roles Created:';
SELECT [Name], [Description], [IsSystemRole] 
FROM [dbo].[AspNetRoles]
ORDER BY [Name];

PRINT '';
PRINT '=============================================';
PRINT 'Next Steps:';
PRINT '1. Update appsettings.json with connection string';
PRINT '2. Configure ASP.NET Core Identity in Program.cs';
PRINT '3. Run user migration utility to import legacy users';
PRINT '=============================================';
GO
