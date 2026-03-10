# Database Setup Guide

## Overview

This guide explains how to set up the database schema for the web-based POS system. The system uses a **dual-schema approach**:

- **`dbo` schema**: Existing legacy tables used by WPF POS (read-only for web app)
- **`web` schema**: New tables specific to web application features

This approach ensures **zero disruption** to the existing WPF POS while enabling new web-specific features.

---

## Prerequisites

- SQL Server 2016 or later
- Existing POS database with legacy `dbo` schema
- SQL Server Management Studio (SSMS) or Azure Data Studio
- Database user with CREATE SCHEMA and CREATE TABLE permissions

---

## Quick Start

### Step 1: Execute Database Script

```sql
-- Open database-scripts.sql in SSMS
-- Ensure you're connected to the POS database
-- Execute the entire script (F5)
```

The script will:
- ✅ Create `web` schema
- ✅ Create 5 new tables with indexes
- ✅ Create 3 maintenance stored procedures
- ✅ Insert 5 default feature flags
- ✅ Provide SQL Agent job templates (optional)

### Step 2: Verify Installation

```sql
-- Check schema exists
SELECT * FROM sys.schemas WHERE name = 'web';

-- Check tables exist
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'web';

-- Check feature flags
SELECT * FROM [web].[FeatureFlags];
```

Expected output:
```
web.OrderLocks
web.ApiAuditLog
web.UserSessions
web.FeatureFlags
web.SyncQueue
```

### Step 3: Configure Permissions (Optional)

If you have a dedicated database user for the web application:

```sql
-- Create database user (if not exists)
CREATE USER [WebPosAppUser] FOR LOGIN [WebPosAppLogin];

-- Grant read-only access to legacy dbo schema
GRANT SELECT ON SCHEMA::[dbo] TO [WebPosAppUser];

-- Grant full access to web schema
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[web] TO [WebPosAppUser];

-- Grant execute on stored procedures
GRANT EXECUTE ON [web].[CleanupExpiredLocks] TO [WebPosAppUser];
GRANT EXECUTE ON [web].[CleanupOldAuditLogs] TO [WebPosAppUser];
GRANT EXECUTE ON [web].[CleanupExpiredSessions] TO [WebPosAppUser];
```

### Step 4: Update Connection String

Update `Pos.Web.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PosDatabase": "Server=YOUR_SERVER;Database=POS;User Id=WebPosAppUser;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

---

## Table Descriptions

### 1. web.OrderLocks

**Purpose**: Prevent concurrent editing of pending orders

**Key Columns**:
- `OrderID`: Reference to `dbo.PendingInvoices`
- `UserID`: User who acquired the lock
- `LockExpiresAt`: When the lock expires (default: 5 minutes)
- `IsActive`: Whether the lock is currently active

**Indexes**:
- `IX_OrderLocks_OrderID_IsActive`: Fast lookup of active locks
- `IX_OrderLocks_LockExpiresAt`: Efficient cleanup of expired locks

**Use Case**: When a cashier opens a pending order, a lock is acquired. Other users see "Order locked by John" and cannot edit until the lock expires or is released.

---

### 2. web.ApiAuditLog

**Purpose**: Track all API operations for security and debugging

**Key Columns**:
- `Action`: Operation performed (e.g., "CreateOrder", "UpdateCustomer")
- `EntityType`: Type of entity (e.g., "Order", "Customer")
- `OldValues` / `NewValues`: JSON snapshots of changes
- `Duration`: API call duration in milliseconds
- `StatusCode`: HTTP status code

**Indexes**:
- `IX_ApiAuditLog_Timestamp`: Fast retrieval of recent logs
- `IX_ApiAuditLog_UserID_Timestamp`: User activity tracking
- `IX_ApiAuditLog_Action_Timestamp`: Action-specific queries

**Use Case**: Audit trail for compliance, debugging slow API calls, tracking user activity.

---

### 3. web.UserSessions

**Purpose**: Manage JWT refresh tokens and active sessions

**Key Columns**:
- `SessionID`: Unique session identifier
- `RefreshToken`: JWT refresh token (hashed)
- `RefreshTokenExpiresAt`: Token expiration (default: 7 days)
- `LastActivityAt`: Last API call timestamp
- `IsActive`: Whether session is active

**Indexes**:
- `IX_UserSessions_RefreshToken`: Fast token lookup
- `IX_UserSessions_RefreshTokenExpiresAt`: Efficient cleanup

**Use Case**: When a user logs in, a session is created. The refresh token allows obtaining new access tokens without re-entering credentials.

---

### 4. web.FeatureFlags

**Purpose**: Enable/disable features dynamically without deployment

**Key Columns**:
- `Name`: Feature flag name (unique)
- `IsEnabled`: Global enable/disable
- `EnabledForUserIDs`: JSON array of specific user IDs
- `EnabledForRoles`: JSON array of role names

**Default Flags**:
- `EnableOfflineMode`: Offline order creation (enabled)
- `EnableKitchenDisplay`: Real-time kitchen updates (enabled)
- `EnableOrderLocking`: Concurrent edit prevention (enabled)
- `EnableLoyaltyPoints`: Loyalty system (disabled)
- `EnableBarcodeScanner`: Barcode scanning (disabled)

**Use Case**: Gradually roll out new features, A/B testing, disable features during incidents.

---

### 5. web.SyncQueue

**Purpose**: Queue offline operations for synchronization

**Key Columns**:
- `OperationType`: Create, Update, Delete
- `EntityType`: Order, Customer, etc.
- `Payload`: JSON representation of operation
- `ClientTimestamp`: When operation occurred offline
- `Status`: Pending, Processing, Completed, Failed
- `AttemptCount`: Number of sync attempts

**Indexes**:
- `IX_SyncQueue_Status_ClientTimestamp`: Process queue in order
- `IX_SyncQueue_UserID_DeviceID_Status`: Device-specific sync

**Use Case**: Waiter creates order on tablet while offline. Order is queued and synced when connection is restored.

---

## Maintenance Stored Procedures

### 1. web.CleanupExpiredLocks

**Purpose**: Deactivate expired order locks

**Schedule**: Every 5 minutes (recommended)

**Manual Execution**:
```sql
EXEC [web].[CleanupExpiredLocks];
```

**What it does**:
- Deactivates locks where `LockExpiresAt < GETUTCDATE()`
- Deletes inactive locks older than 7 days

---

### 2. web.CleanupOldAuditLogs

**Purpose**: Delete old audit logs to manage database size

**Schedule**: Daily at 2 AM (recommended)

**Manual Execution**:
```sql
-- Delete logs older than 90 days (default)
EXEC [web].[CleanupOldAuditLogs] @RetentionDays = 90;

-- Delete logs older than 30 days
EXEC [web].[CleanupOldAuditLogs] @RetentionDays = 30;
```

**What it does**:
- Deletes audit logs older than specified retention period
- Default: 90 days

---

### 3. web.CleanupExpiredSessions

**Purpose**: Deactivate expired user sessions

**Schedule**: Every hour (recommended)

**Manual Execution**:
```sql
EXEC [web].[CleanupExpiredSessions];
```

**What it does**:
- Deactivates sessions where `RefreshTokenExpiresAt < GETUTCDATE()`
- Deletes inactive sessions older than 30 days

---

## SQL Agent Jobs (Optional)

For automated maintenance, configure SQL Server Agent jobs:

### Enable SQL Agent Jobs

1. Open SSMS
2. Connect to SQL Server
3. Expand **SQL Server Agent** → **Jobs**
4. Uncomment section 9 in `database-scripts.sql`
5. Execute the SQL Agent job creation script

### Verify Jobs

```sql
-- Check job status
SELECT 
    j.name AS JobName,
    j.enabled AS IsEnabled,
    s.name AS ScheduleName,
    CASE s.freq_type
        WHEN 4 THEN 'Daily'
        WHEN 8 THEN 'Weekly'
        ELSE 'Other'
    END AS Frequency
FROM msdb.dbo.sysjobs j
INNER JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
INNER JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
WHERE j.name LIKE 'Web POS%';
```

---

## Monitoring and Troubleshooting

### Check Active Order Locks

```sql
SELECT 
    ol.ID,
    ol.OrderID,
    u.Name AS LockedBy,
    ol.LockAcquiredAt,
    ol.LockExpiresAt,
    DATEDIFF(SECOND, GETUTCDATE(), ol.LockExpiresAt) AS SecondsRemaining
FROM [web].[OrderLocks] ol
INNER JOIN [dbo].[Users] u ON ol.UserID = u.ID
WHERE ol.IsActive = 1
ORDER BY ol.LockAcquiredAt DESC;
```

### Check Recent API Activity

```sql
SELECT TOP 100
    al.Timestamp,
    u.Name AS UserName,
    al.Action,
    al.EntityType,
    al.StatusCode,
    al.Duration AS DurationMs,
    al.ErrorMessage
FROM [web].[ApiAuditLog] al
LEFT JOIN [dbo].[Users] u ON al.UserID = u.ID
ORDER BY al.Timestamp DESC;
```

### Check Active Sessions

```sql
SELECT 
    us.SessionID,
    u.Name AS UserName,
    us.DeviceInfo,
    us.IPAddress,
    us.LastActivityAt,
    DATEDIFF(MINUTE, us.LastActivityAt, GETUTCDATE()) AS MinutesSinceActivity,
    us.RefreshTokenExpiresAt
FROM [web].[UserSessions] us
INNER JOIN [dbo].[Users] u ON us.UserID = u.ID
WHERE us.IsActive = 1
ORDER BY us.LastActivityAt DESC;
```

### Check Sync Queue Status

```sql
SELECT 
    sq.Status,
    COUNT(*) AS Count,
    MIN(sq.ClientTimestamp) AS OldestOperation,
    MAX(sq.ClientTimestamp) AS NewestOperation
FROM [web].[SyncQueue] sq
GROUP BY sq.Status;
```

### Check Feature Flags

```sql
SELECT 
    Name,
    Description,
    IsEnabled,
    UpdatedAt,
    u.Name AS UpdatedBy
FROM [web].[FeatureFlags] ff
LEFT JOIN [dbo].[Users] u ON ff.UpdatedBy = u.ID
ORDER BY Name;
```

---

## Performance Considerations

### Index Maintenance

```sql
-- Rebuild indexes monthly
ALTER INDEX ALL ON [web].[OrderLocks] REBUILD;
ALTER INDEX ALL ON [web].[ApiAuditLog] REBUILD;
ALTER INDEX ALL ON [web].[UserSessions] REBUILD;
ALTER INDEX ALL ON [web].[SyncQueue] REBUILD;

-- Update statistics
UPDATE STATISTICS [web].[OrderLocks];
UPDATE STATISTICS [web].[ApiAuditLog];
UPDATE STATISTICS [web].[UserSessions];
UPDATE STATISTICS [web].[SyncQueue];
```

### Table Size Monitoring

```sql
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB,
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.schema_id = SCHEMA_ID('web')
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceKB DESC;
```

---

## Rollback Procedure

If you need to remove the web schema:

```sql
-- WARNING: This will delete all web-specific data!

-- Drop stored procedures
DROP PROCEDURE IF EXISTS [web].[CleanupExpiredLocks];
DROP PROCEDURE IF EXISTS [web].[CleanupOldAuditLogs];
DROP PROCEDURE IF EXISTS [web].[CleanupExpiredSessions];

-- Drop tables (in order due to foreign keys)
DROP TABLE IF EXISTS [web].[SyncQueue];
DROP TABLE IF EXISTS [web].[FeatureFlags];
DROP TABLE IF EXISTS [web].[UserSessions];
DROP TABLE IF EXISTS [web].[ApiAuditLog];
DROP TABLE IF EXISTS [web].[OrderLocks];

-- Drop schema
DROP SCHEMA IF EXISTS [web];

PRINT 'Web schema removed successfully';
```

---

## Next Steps

After database setup:

1. ✅ Update connection string in `appsettings.json`
2. ✅ Run Entity Framework migrations (if using Code First)
3. ✅ Test database connectivity from API
4. ✅ Configure SQL Agent jobs for maintenance
5. ✅ Set up database backups
6. ✅ Configure monitoring and alerts

---

## Support

For issues or questions:
- Check the troubleshooting section above
- Review audit logs for errors
- Contact the development team

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-28  
**Database Version**: 1.0.0
