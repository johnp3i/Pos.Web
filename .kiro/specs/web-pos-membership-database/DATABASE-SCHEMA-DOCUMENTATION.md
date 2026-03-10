# WebPosMembership Database Schema Documentation

**Database Name:** WebPosMembership  
**Database Engine:** Microsoft SQL Server  
**Version:** 1.0.0  
**Last Updated:** March 4, 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Tables](#tables)
4. [Indexes](#indexes)
5. [Relationships](#relationships)
6. [Migration Scripts](#migration-scripts)

---

## Overview

The WebPosMembership database provides authentication and authorization services for the MyChair Web POS application. It uses ASP.NET Core Identity for user management with custom extensions for session tracking, audit logging, and integration with the legacy POS system.

**Key Features:**
- ASP.NET Core Identity integration
- JWT token-based authentication
- Session management and tracking
- Comprehensive audit logging
- Password history tracking
- Refresh token management
- Cross-database foreign key to legacy POS system

---

## Entity Relationship Diagram

```
┌─────────────────┐
│  AspNetUsers    │
│  (Identity)     │
├─────────────────┤
│ Id (PK)         │
│ UserName        │
│ Email           │
│ EmployeeId (FK) │──┐
│ DisplayName     │  │
│ IsActive        │  │
│ CreatedAt       │  │
│ LastLoginAt     │  │
└────────┬────────┘  │
         │           │
         │           │ Cross-DB FK
         │           │
    ┌────┴────┐      │
    │         │      │
    ▼         ▼      ▼
┌──────────┐ ┌──────────┐ ┌──────────────┐
│RefreshTok│ │UserSessi │ │ POS.dbo.Users│
│ens       │ │ons       │ │ (Legacy)     │
├──────────┤ ├──────────┤ ├──────────────┤
│Id (PK)   │ │Id (PK)   │ │ID (PK)       │
│Token     │ │SessionId │ │Name          │
│UserId(FK)│ │UserId(FK)│ │FullName      │
│ExpiresAt │ │DeviceType│ │PositionTypeID│
│RevokedAt │ │IpAddress │ │IsActive      │
└──────────┘ └──────────┘ └──────────────┘
    │            │
    │            │
    ▼            ▼
┌──────────┐ ┌──────────┐
│AuthAudit │ │Password  │
│Log       │ │History   │
├──────────┤ ├──────────┤
│Id (PK)   │ │Id (PK)   │
│UserId(FK)│ │UserId(FK)│
│EventType │ │PasswordH │
│Timestamp │ │CreatedAt │
│IpAddress │ │ChangedBy │
└──────────┘ └──────────┘

┌─────────────────┐
│  AspNetRoles    │
│  (Identity)     │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Description     │
│ IsSystemRole    │
└─────────────────┘
         │
         │ Many-to-Many
         ▼
┌─────────────────┐
│AspNetUserRoles  │
│  (Identity)     │
├─────────────────┤
│ UserId (FK)     │
│ RoleId (FK)     │
└─────────────────┘
```

---

## Tables

### AspNetUsers

ASP.NET Core Identity user table with custom properties.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | nvarchar(450) | No | Primary key (GUID) |
| UserName | nvarchar(256) | No | Unique username |
| NormalizedUserName | nvarchar(256) | No | Uppercase username for lookups |
| Email | nvarchar(256) | Yes | User email address |
| NormalizedEmail | nvarchar(256) | Yes | Uppercase email for lookups |
| EmailConfirmed | bit | No | Email confirmation status |
| PasswordHash | nvarchar(MAX) | Yes | Hashed password |
| SecurityStamp | nvarchar(MAX) | Yes | Security stamp for token invalidation |
| ConcurrencyStamp | nvarchar(MAX) | Yes | Concurrency token |
| PhoneNumber | nvarchar(MAX) | Yes | Phone number |
| PhoneNumberConfirmed | bit | No | Phone confirmation status |
| TwoFactorEnabled | bit | No | 2FA enabled flag |
| LockoutEnd | datetimeoffset | Yes | Lockout expiration time |
| LockoutEnabled | bit | No | Lockout feature enabled |
| AccessFailedCount | int | No | Failed login attempt count |
| **EmployeeId** | int | No | **Foreign key to POS.dbo.Users.ID** |
| **FirstName** | nvarchar(100) | Yes | **User first name** |
| **LastName** | nvarchar(100) | Yes | **User last name** |
| **DisplayName** | nvarchar(200) | Yes | **Full display name** |
| **IsActive** | bit | No | **Account active status** |
| **CreatedAt** | datetime2 | No | **Account creation timestamp** |
| **LastLoginAt** | datetime2 | Yes | **Last successful login** |
| **LastPasswordChangedAt** | datetime2 | Yes | **Last password change** |
| **RequirePasswordChange** | bit | No | **Force password change flag** |
| **IsTwoFactorEnabled** | bit | No | **2FA enabled (custom)** |

**Indexes:**
- `IX_AspNetUsers_NormalizedUserName` (Unique)
- `IX_AspNetUsers_NormalizedEmail`
- `IX_AspNetUsers_EmployeeId` (Unique)
- `IX_AspNetUsers_IsActive`

**Constraints:**
- `PK_AspNetUsers` (Primary Key on Id)
- `FK_AspNetUsers_POS_Users` (Foreign Key to POS.dbo.Users.ID)

---

### AspNetRoles

ASP.NET Core Identity role table with custom properties.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | nvarchar(450) | No | Primary key (GUID) |
| Name | nvarchar(256) | No | Role name |
| NormalizedName | nvarchar(256) | No | Uppercase role name |
| ConcurrencyStamp | nvarchar(MAX) | Yes | Concurrency token |
| **Description** | nvarchar(500) | Yes | **Role description** |
| **CreatedAt** | datetime2 | No | **Role creation timestamp** |
| **IsSystemRole** | bit | No | **System-defined role flag** |

**Indexes:**
- `IX_AspNetRoles_NormalizedName` (Unique)

**System Roles:**
- Admin
- Manager
- Cashier
- Waiter
- Kitchen

---

### AspNetUserRoles

Many-to-many relationship between users and roles.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| UserId | nvarchar(450) | No | Foreign key to AspNetUsers |
| RoleId | nvarchar(450) | No | Foreign key to AspNetRoles |

**Indexes:**
- `PK_AspNetUserRoles` (Composite Primary Key)
- `IX_AspNetUserRoles_RoleId`

---

### RefreshTokens

Stores refresh tokens for JWT authentication.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (Identity) |
| Token | nvarchar(500) | No | Unique refresh token |
| UserId | nvarchar(450) | No | Foreign key to AspNetUsers |
| ExpiresAt | datetime2 | No | Token expiration time (7 days) |
| CreatedAt | datetime2 | No | Token creation time |
| RevokedAt | datetime2 | Yes | Token revocation time |
| RevokedReason | nvarchar(500) | Yes | Reason for revocation |
| ReplacedByToken | nvarchar(500) | Yes | New token that replaced this one |
| DeviceInfo | nvarchar(500) | Yes | Device information |
| IpAddress | nvarchar(45) | Yes | IP address |

**Indexes:**
- `PK_RefreshTokens` (Primary Key)
- `IX_RefreshTokens_Token` (Unique)
- `IX_RefreshTokens_UserId`
- `IX_RefreshTokens_ExpiresAt`
- `IX_RefreshTokens_RevokedAt`

**Constraints:**
- `FK_RefreshTokens_AspNetUsers` (Cascade Delete)

---

### UserSessions

Tracks active user sessions.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (Identity) |
| SessionId | uniqueidentifier | No | Unique session identifier (GUID) |
| UserId | nvarchar(450) | No | Foreign key to AspNetUsers |
| DeviceType | nvarchar(50) | No | Desktop, Tablet, or Mobile |
| DeviceInfo | nvarchar(500) | Yes | Browser and OS information |
| IpAddress | nvarchar(45) | Yes | IP address |
| UserAgent | nvarchar(1000) | Yes | Full user agent string |
| CreatedAt | datetime2 | No | Session start time |
| LastActivityAt | datetime2 | No | Last activity timestamp |
| EndedAt | datetime2 | Yes | Session end time (null = active) |

**Indexes:**
- `PK_UserSessions` (Primary Key)
- `IX_UserSessions_SessionId` (Unique)
- `IX_UserSessions_UserId`
- `IX_UserSessions_CreatedAt`
- `IX_UserSessions_EndedAt`

**Constraints:**
- `FK_UserSessions_AspNetUsers` (Cascade Delete)
- `CK_UserSessions_DeviceType` (Check: DeviceType IN ('Desktop', 'Tablet', 'Mobile'))

---

### AuthAuditLog

Comprehensive audit log for all authentication events.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | bigint | No | Primary key (Identity) |
| EventType | nvarchar(100) | No | Type of event (LoginSuccess, LoginFailed, etc.) |
| UserId | nvarchar(450) | Yes | Foreign key to AspNetUsers (nullable) |
| UserName | nvarchar(256) | Yes | Username (for failed logins) |
| Timestamp | datetime2 | No | Event timestamp |
| IpAddress | nvarchar(45) | Yes | IP address |
| UserAgent | nvarchar(1000) | Yes | User agent string |
| Details | nvarchar(MAX) | Yes | Additional event details (JSON) |
| IsSuccessful | bit | No | Success/failure flag |
| ErrorMessage | nvarchar(1000) | Yes | Error message for failures |

**Indexes:**
- `PK_AuthAuditLog` (Primary Key)
- `IX_AuthAuditLog_UserId`
- `IX_AuthAuditLog_EventType`
- `IX_AuthAuditLog_Timestamp`
- `IX_AuthAuditLog_IsSuccessful`

**Constraints:**
- `FK_AuthAuditLog_AspNetUsers` (Set Null on Delete)

**Event Types:**
- LoginSuccess
- LoginFailed
- Logout
- PasswordChanged
- AccountLocked
- AccountUnlocked
- TokenRefreshed
- TokenRevoked
- SessionCreated
- SessionEnded
- RoleChanged
- UserCreated
- UserDeactivated
- SecurityEvent

---

### PasswordHistory

Tracks password history to prevent reuse.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (Identity) |
| UserId | nvarchar(450) | No | Foreign key to AspNetUsers |
| PasswordHash | nvarchar(MAX) | No | Hashed password |
| CreatedAt | datetime2 | No | Password creation time |
| ChangedBy | nvarchar(450) | Yes | User who changed the password |
| ChangeReason | nvarchar(500) | Yes | Reason for change |

**Indexes:**
- `PK_PasswordHistory` (Primary Key)
- `IX_PasswordHistory_UserId`
- `IX_PasswordHistory_CreatedAt`

**Constraints:**
- `FK_PasswordHistory_AspNetUsers` (Cascade Delete)

**Business Rule:** System keeps last 5 passwords per user

---

### ApplicationLogs

Serilog structured logging table.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (Identity) |
| Message | nvarchar(MAX) | Yes | Log message |
| MessageTemplate | nvarchar(MAX) | Yes | Message template |
| Level | nvarchar(128) | Yes | Log level (Information, Warning, Error) |
| TimeStamp | datetime | No | Log timestamp |
| Exception | nvarchar(MAX) | Yes | Exception details |
| Properties | nvarchar(MAX) | Yes | Additional properties (XML) |
| **CorrelationId** | nvarchar(50) | Yes | **Request correlation ID** |
| **UserId** | nvarchar(450) | Yes | **User ID** |
| **UserName** | nvarchar(256) | Yes | **Username** |
| **IpAddress** | nvarchar(45) | Yes | **IP address** |

**Indexes:**
- `PK_ApplicationLogs` (Primary Key)
- `IX_ApplicationLogs_TimeStamp`
- `IX_ApplicationLogs_Level`

---

## Indexes

### Performance Indexes

| Table | Index Name | Columns | Type | Purpose |
|-------|------------|---------|------|---------|
| AspNetUsers | IX_AspNetUsers_EmployeeId | EmployeeId | Unique | Legacy system integration |
| AspNetUsers | IX_AspNetUsers_IsActive | IsActive | Non-Unique | Active user queries |
| RefreshTokens | IX_RefreshTokens_Token | Token | Unique | Token validation |
| RefreshTokens | IX_RefreshTokens_ExpiresAt | ExpiresAt | Non-Unique | Cleanup operations |
| UserSessions | IX_UserSessions_SessionId | SessionId | Unique | Session lookup |
| UserSessions | IX_UserSessions_EndedAt | EndedAt | Non-Unique | Active session queries |
| AuthAuditLog | IX_AuthAuditLog_Timestamp | Timestamp | Non-Unique | Time-based queries |
| AuthAuditLog | IX_AuthAuditLog_EventType | EventType | Non-Unique | Event filtering |
| PasswordHistory | IX_PasswordHistory_CreatedAt | CreatedAt DESC | Non-Unique | Recent password queries |

---

## Relationships

### One-to-Many Relationships

1. **AspNetUsers → RefreshTokens**
   - One user can have multiple refresh tokens
   - Cascade delete: When user is deleted, all tokens are deleted

2. **AspNetUsers → UserSessions**
   - One user can have multiple sessions
   - Cascade delete: When user is deleted, all sessions are deleted

3. **AspNetUsers → PasswordHistory**
   - One user can have multiple password history entries
   - Cascade delete: When user is deleted, all history is deleted

4. **AspNetUsers → AuthAuditLog**
   - One user can have multiple audit log entries
   - Set null on delete: Audit logs are preserved even if user is deleted

### Many-to-Many Relationships

1. **AspNetUsers ↔ AspNetRoles** (via AspNetUserRoles)
   - Users can have multiple roles
   - Roles can be assigned to multiple users

### Cross-Database Relationships

1. **AspNetUsers.EmployeeId → POS.dbo.Users.ID**
   - Links identity user to legacy POS user
   - Unique constraint ensures one-to-one mapping
   - Foreign key enforced at application level (not database level due to cross-database limitation)

---

## Migration Scripts

### Initial Migration

```sql
-- Create database
CREATE DATABASE WebPosMembership;
GO

USE WebPosMembership;
GO

-- Run Entity Framework migrations
-- dotnet ef database update --context WebPosMembershipDbContext
```

### Seed System Roles

```sql
INSERT INTO AspNetRoles (Id, Name, NormalizedName, Description, CreatedAt, IsSystemRole)
VALUES
    (NEWID(), 'Admin', 'ADMIN', 'System administrator with full access', GETUTCDATE(), 1),
    (NEWID(), 'Manager', 'MANAGER', 'Manager with elevated permissions', GETUTCDATE(), 1),
    (NEWID(), 'Cashier', 'CASHIER', 'Cashier with POS access', GETUTCDATE(), 1),
    (NEWID(), 'Waiter', 'WAITER', 'Waiter with order management access', GETUTCDATE(), 1),
    (NEWID(), 'Kitchen', 'KITCHEN', 'Kitchen staff with order viewing access', GETUTCDATE(), 1);
```

### Cleanup Expired Tokens

```sql
-- Delete refresh tokens expired more than 30 days ago
DELETE FROM RefreshTokens
WHERE ExpiresAt < DATEADD(DAY, -30, GETUTCDATE());
```

### Archive Old Audit Logs

```sql
-- Archive audit logs older than 1 year
INSERT INTO AuthAuditLog_Archive
SELECT * FROM AuthAuditLog
WHERE Timestamp < DATEADD(YEAR, -1, GETUTCDATE());

DELETE FROM AuthAuditLog
WHERE Timestamp < DATEADD(YEAR, -1, GETUTCDATE());
```

---

## Backup and Restore

### Backup Strategy

```sql
-- Full backup (daily)
BACKUP DATABASE WebPosMembership
TO DISK = 'C:\Backups\WebPosMembership_Full.bak'
WITH FORMAT, COMPRESSION;

-- Differential backup (hourly)
BACKUP DATABASE WebPosMembership
TO DISK = 'C:\Backups\WebPosMembership_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION;

-- Transaction log backup (every 15 minutes)
BACKUP LOG WebPosMembership
TO DISK = 'C:\Backups\WebPosMembership_Log.trn'
WITH COMPRESSION;
```

### Restore Procedure

```sql
-- Restore full backup
RESTORE DATABASE WebPosMembership
FROM DISK = 'C:\Backups\WebPosMembership_Full.bak'
WITH NORECOVERY;

-- Restore differential backup
RESTORE DATABASE WebPosMembership
FROM DISK = 'C:\Backups\WebPosMembership_Diff.bak'
WITH NORECOVERY;

-- Restore transaction log
RESTORE LOG WebPosMembership
FROM DISK = 'C:\Backups\WebPosMembership_Log.trn'
WITH RECOVERY;
```

---

## Maintenance Tasks

### Daily Tasks

1. Backup database (full backup)
2. Check database integrity
3. Update statistics

```sql
-- Check database integrity
DBCC CHECKDB (WebPosMembership) WITH NO_INFOMSGS;

-- Update statistics
EXEC sp_updatestats;
```

### Weekly Tasks

1. Rebuild fragmented indexes
2. Clean up expired tokens
3. Archive old audit logs

```sql
-- Rebuild indexes with fragmentation > 30%
ALTER INDEX ALL ON AspNetUsers REBUILD;
ALTER INDEX ALL ON RefreshTokens REBUILD;
ALTER INDEX ALL ON UserSessions REBUILD;
ALTER INDEX ALL ON AuthAuditLog REBUILD;
```

### Monthly Tasks

1. Archive audit logs older than 1 year
2. Review and optimize slow queries
3. Check database growth and plan capacity

---

## Performance Tuning

### Query Optimization Tips

1. **Use indexes effectively:**
   - Filter by IsActive when querying users
   - Use SessionId for session lookups
   - Filter by Timestamp for audit log queries

2. **Avoid N+1 queries:**
   - Use `.Include()` for related entities
   - Use `.AsNoTracking()` for read-only queries

3. **Batch operations:**
   - Use bulk insert for audit logs
   - Batch delete for cleanup operations

4. **Connection pooling:**
   - Min Pool Size: 5
   - Max Pool Size: 100
   - Connection Timeout: 30 seconds

---

## Security Considerations

1. **Encryption:**
   - Enable Transparent Data Encryption (TDE) for production
   - Use encrypted connections (Encrypt=True in connection string)

2. **Access Control:**
   - Use least privilege principle for database users
   - Separate read-only and read-write accounts

3. **Audit:**
   - Enable SQL Server audit for DDL changes
   - Monitor failed login attempts

4. **Backup:**
   - Encrypt backup files
   - Store backups in secure location
   - Test restore procedures regularly

---

## Troubleshooting

### Common Issues

1. **Slow login queries:**
   - Check IX_AspNetUsers_NormalizedUserName index
   - Verify statistics are up to date

2. **Session cleanup not running:**
   - Check SessionCleanupService is running
   - Verify IX_UserSessions_EndedAt index exists

3. **Audit log table growing too large:**
   - Implement archival process
   - Consider partitioning by Timestamp

4. **Cross-database foreign key violations:**
   - Verify EmployeeId exists in POS.dbo.Users
   - Check application-level validation

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-03-04 | Initial schema creation |

---

## Contact

For database schema questions or issues:
- **DBA Team:** dba@yourcompany.com
- **Documentation:** https://docs.yourcompany.com/database
