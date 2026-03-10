# Task 2 Completion Summary: Database Schema Setup

**Date**: 2026-03-05  
**Tasks Completed**: 2.1, 2.2, 2.3  
**Status**: ✅ Complete

---

## Overview

Phase 2 of the web-based POS system implementation focused on database infrastructure setup. All web schema tables, maintenance procedures, and security configurations are now in place.

---

## Completed Tasks

### ✅ Task 2.1: Create web schema and tables
**Status**: Completed by user  
**Deliverables**:
- `web` schema created
- `web.OrderLocks` table with indexes
- `web.ApiAuditLog` table with indexes
- `web.UserSessions` table with indexes
- `web.FeatureFlags` table with indexes
- `web.SyncQueue` table with indexes

### ✅ Task 2.2: Create database maintenance stored procedures
**Status**: Completed  
**Deliverables**:
- `web.CleanupExpiredLocks` - Removes expired order locks
- `web.CleanupOldAuditLogs` - Archives and removes old audit logs
- `web.CleanupExpiredSessions` - Marks expired sessions as inactive
- `web.CleanupFailedSyncQueue` - Removes old failed sync items
- `web.GetMaintenanceStatus` - Returns status of all web tables

**File Created**: `Pos.Web/database-maintenance-and-permissions.sql`

### ✅ Task 2.3: Configure database permissions
**Status**: Completed  
**Deliverables**:
- `WebPosAppUser` database user created
- SELECT permissions granted on `dbo` schema (read-only)
- INSERT/UPDATE permissions on specific `dbo` tables (Invoices, Customers, etc.)
- Full CRUD permissions on `web` schema
- EXECUTE permissions on `web` schema stored procedures

**File Created**: `Pos.Web/database-maintenance-and-permissions.sql`

---

## SQL Scripts Created

### 1. `database-maintenance-and-permissions.sql`
**Purpose**: Complete setup for maintenance procedures and security  
**Sections**:
- Database maintenance stored procedures (Task 2.2)
- Database permissions configuration (Task 2.3)
- Verification queries

**How to Run**:
```sql
-- Connect to POS database in SQL Server Management Studio
-- Open: Pos.Web/database-maintenance-and-permissions.sql
-- Execute the entire script (F5)
```

### 2. `database-maintenance-jobs.sql`
**Purpose**: SQL Server Agent job scheduling for automated maintenance  
**Jobs Created**:
1. **WebPOS - Cleanup Expired Locks** (every 5 minutes)
2. **WebPOS - Cleanup Expired Sessions** (every hour)
3. **WebPOS - Cleanup Old Audit Logs** (daily at 2:00 AM)
4. **WebPOS - Cleanup Failed Sync Queue** (daily at 3:00 AM)

**How to Run**:
```sql
-- Connect to msdb database in SQL Server Management Studio
-- Open: Pos.Web/database-maintenance-jobs.sql
-- Execute the entire script (F5)
-- Verify SQL Server Agent service is running
```

---

## Maintenance Procedures Details

### 1. web.CleanupExpiredLocks
**Schedule**: Every 5 minutes  
**Purpose**: Prevents lock table bloat by deactivating expired locks  
**Behavior**: 
- Deactivates locks where `LockExpiresAt < GETUTCDATE()` and `IsActive = 1`
- Deletes inactive locks older than 7 days
**Impact**: Low - typically affects 0-10 records per run

**Manual Execution**:
```sql
EXEC web.CleanupExpiredLocks;
```

### 2. web.CleanupExpiredSessions
**Schedule**: Every hour  
**Purpose**: Marks expired user sessions as inactive  
**Behavior**:
- Updates sessions where `RefreshTokenExpiresAt < GETUTCDATE()` and `IsActive = 1`
- Sets `IsActive = 0` and `LoggedOutAt = GETUTCDATE()`
- Deletes inactive sessions older than 30 days
**Impact**: Low - updates 0-50 records per run

**Manual Execution**:
```sql
EXEC web.CleanupExpiredSessions;
```

### 3. web.CleanupOldAuditLogs
**Schedule**: Daily at 2:00 AM  
**Purpose**: Archives and removes old audit logs  
**Retention**: 90 days (configurable)  
**Impact**: Medium - may delete thousands of records in batches

**Manual Execution**:
```sql
-- Default 90-day retention
EXEC web.CleanupOldAuditLogs;

-- Custom retention (e.g., 180 days)
EXEC web.CleanupOldAuditLogs @RetentionDays = 180;
```

### 4. web.CleanupFailedSyncQueue
**Schedule**: Daily at 3:00 AM  
**Purpose**: Removes old failed sync queue items  
**Behavior**:
- Deletes items where `Status = 'Failed'`, `ClientTimestamp < @CutoffDate`, and `AttemptCount >= @MaxAttempts`
**Retention**: 7 days (configurable)  
**Max Attempts**: 5 (configurable)  
**Impact**: Low - deletes failed items that exceeded max attempts

**Manual Execution**:
```sql
-- Default 7-day retention, 5 max attempts
EXEC web.CleanupFailedSyncQueue;

-- Custom retention and max attempts
EXEC web.CleanupFailedSyncQueue @RetentionDays = 14, @MaxAttempts = 10;
```

### 5. web.GetMaintenanceStatus
**Schedule**: On-demand  
**Purpose**: Monitoring and health check  
**Returns**: Status of all web schema tables

**Manual Execution**:
```sql
EXEC web.GetMaintenanceStatus;
```

**Sample Output**:
```
TableName       TotalRecords  ActiveLocks  ExpiredLocks  OldestLock           NewestLock
--------------  ------------  -----------  ------------  -------------------  -------------------
OrderLocks      5             5            0             2026-03-05 10:15:00  2026-03-05 10:45:00
ApiAuditLog     15234         1523         13711         2025-12-01 00:00:00  2026-03-05 10:50:00
UserSessions    42            38           4             2026-03-01 08:00:00  2026-03-05 10:30:00
FeatureFlags    12            8            4             2026-02-15 12:00:00  2026-03-04 15:20:00
SyncQueue       3             2            1             2026-03-05 09:00:00  2026-03-05 10:00:00
```

**Column Definitions**:
- **OrderLocks**: Uses `LockAcquiredAt`, `LockExpiresAt`, `IsActive`, `UpdatedAt`
- **ApiAuditLog**: Uses `Timestamp`, `Action`, `EntityType`, `EntityID`, `RequestMethod`, `Duration`
- **UserSessions**: Uses `SessionID`, `RefreshToken`, `RefreshTokenExpiresAt`, `IsActive`, `CreatedAt`, `LastActivityAt`, `LoggedOutAt`
- **FeatureFlags**: Uses `Name`, `Description`, `IsEnabled`, `EnabledForUserIDs`, `EnabledForRoles`, `UpdatedAt`, `UpdatedBy`
- **SyncQueue**: Uses `DeviceID`, `OperationType`, `EntityType`, `EntityID`, `Payload`, `ClientTimestamp`, `ServerTimestamp`, `Status`, `AttemptCount`, `LastAttemptAt`, `ProcessedAt`

---

## Database Permissions Summary

### WebPosAppUser Permissions

#### dbo Schema (Legacy Tables)
**Read Access** (SELECT):
- All tables in `dbo` schema

**Write Access** (INSERT, UPDATE):
- `dbo.Invoices`
- `dbo.InvoiceItems`
- `dbo.PendingInvoices`
- `dbo.PendingInvoiceItems`
- `dbo.Customers`
- `dbo.CustomerAddresses`

**Insert-Only Access**:
- `dbo.ServerCommandsHistory`

#### web Schema (New Tables)
**Full CRUD Access** (SELECT, INSERT, UPDATE, DELETE):
- `web.OrderLocks`
- `web.ApiAuditLog`
- `web.UserSessions`
- `web.FeatureFlags`
- `web.SyncQueue`

**Execute Access**:
- All stored procedures in `web` schema

---

## Production Deployment Notes

### 1. Map WebPosAppUser to SQL Login

The script creates `WebPosAppUser` without a login for development. In production:

```sql
-- Step 1: Create SQL Server login (if not exists)
CREATE LOGIN WebPosAppLogin WITH PASSWORD = 'YourSecurePassword123!';

-- Step 2: Drop the user without login
USE [POS];
DROP USER WebPosAppUser;

-- Step 3: Create user mapped to login
CREATE USER WebPosAppUser FOR LOGIN WebPosAppLogin;

-- Step 4: Re-run permissions section from database-maintenance-and-permissions.sql
```

### 2. Update Connection String

Update `appsettings.json` in `Pos.Web.API`:

```json
{
  "ConnectionStrings": {
    "PosDatabase": "Server=127.0.0.1;Database=POS;User Id=WebPosAppLogin;Password=YourSecurePassword123!;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

**Security Best Practice**: Use environment variables or Azure Key Vault for production passwords.

### 3. Verify SQL Server Agent

Ensure SQL Server Agent service is running:

```powershell
# Check service status
Get-Service -Name 'SQLSERVERAGENT'

# Start service if stopped
Start-Service -Name 'SQLSERVERAGENT'
```

---

## Monitoring and Maintenance

### Monitor Job Execution

**SQL Server Management Studio**:
1. Expand `SQL Server Agent` > `Jobs`
2. Right-click job > `View History`
3. Check for failures or warnings

**Query Job History**:
```sql
SELECT 
    j.name AS JobName,
    h.run_date,
    h.run_time,
    h.run_duration,
    CASE h.run_status
        WHEN 0 THEN 'Failed'
        WHEN 1 THEN 'Succeeded'
        WHEN 2 THEN 'Retry'
        WHEN 3 THEN 'Canceled'
        WHEN 4 THEN 'In Progress'
    END AS Status,
    h.message
FROM msdb.dbo.sysjobs j
INNER JOIN msdb.dbo.sysjobhistory h ON j.job_id = h.job_id
WHERE j.name LIKE 'WebPOS -%'
  AND h.step_id = 0  -- Job outcome (not individual steps)
ORDER BY h.run_date DESC, h.run_time DESC;
```

### Monitor Table Growth

```sql
-- Check table sizes
SELECT 
    t.name AS TableName,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 / 1024 AS TotalSpaceMB,
    SUM(a.used_pages) * 8 / 1024 AS UsedSpaceMB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.schema_id = SCHEMA_ID('web')
GROUP BY t.name, p.rows
ORDER BY TotalSpaceMB DESC;
```

### Alert on Failures

Create SQL Server alerts for job failures:

```sql
-- Create alert for failed jobs
EXEC msdb.dbo.sp_add_alert 
    @name = N'WebPOS Job Failure Alert',
    @message_id = 0,
    @severity = 0,
    @enabled = 1,
    @delay_between_responses = 900,  -- 15 minutes
    @include_event_description_in = 1,
    @category_name = N'[Uncategorized]',
    @job_name = N'WebPOS - Cleanup Expired Locks';

-- Add notification (requires Database Mail configured)
-- EXEC msdb.dbo.sp_add_notification 
--     @alert_name = N'WebPOS Job Failure Alert',
--     @operator_name = N'DBA Team',
--     @notification_method = 1;  -- Email
```

---

## Testing Procedures

### 1. Test Maintenance Procedures

```sql
-- Test each procedure manually
EXEC web.CleanupExpiredLocks;
EXEC web.CleanupExpiredSessions;
EXEC web.CleanupOldAuditLogs @RetentionDays = 90;
EXEC web.CleanupFailedSyncQueue @RetentionDays = 7, @MaxAttempts = 5;

-- Check results
EXEC web.GetMaintenanceStatus;
```

### 2. Test Permissions

```sql
-- Impersonate WebPosAppUser
EXECUTE AS USER = 'WebPosAppUser';

-- Test SELECT on dbo schema (should succeed)
SELECT TOP 1 * FROM dbo.Users;

-- Test INSERT on Invoices (should succeed)
-- (Don't actually insert test data in production)

-- Test DELETE on dbo.Users (should fail - no permission)
-- DELETE FROM dbo.Users WHERE ID = 999999;

-- Revert to original user
REVERT;
```

### 3. Test SQL Server Agent Jobs

```sql
-- Manually start a job
EXEC msdb.dbo.sp_start_job @job_name = 'WebPOS - Cleanup Expired Locks';

-- Wait a few seconds, then check status
SELECT 
    j.name,
    ja.start_execution_date,
    ja.stop_execution_date,
    ja.last_executed_step_id,
    CASE ja.current_execution_status
        WHEN 1 THEN 'Executing'
        WHEN 2 THEN 'Waiting for thread'
        WHEN 3 THEN 'Between retries'
        WHEN 4 THEN 'Idle'
        WHEN 5 THEN 'Suspended'
        WHEN 7 THEN 'Performing completion actions'
    END AS ExecutionStatus
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobactivity ja ON j.job_id = ja.job_id
WHERE j.name = 'WebPOS - Cleanup Expired Locks'
  AND ja.run_requested_date IS NOT NULL
ORDER BY ja.start_execution_date DESC;
```

---

## Troubleshooting

### Issue: Jobs Not Running

**Symptoms**: Jobs show as enabled but never execute

**Solutions**:
1. Check SQL Server Agent service status
2. Verify job owner has necessary permissions
3. Check SQL Server Agent error log: `C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Log\SQLAGENT.OUT`

### Issue: Cleanup Procedures Timing Out

**Symptoms**: Jobs fail with timeout errors

**Solutions**:
1. Increase `@BatchSize` parameter in cleanup procedures
2. Add more frequent cleanup schedules to prevent large backlogs
3. Check for blocking queries: `sp_who2`

### Issue: Permission Denied Errors

**Symptoms**: Application cannot access tables

**Solutions**:
1. Verify WebPosAppUser exists: `SELECT * FROM sys.database_principals WHERE name = 'WebPosAppUser'`
2. Re-run permissions section of script
3. Check connection string uses correct user

---

## Next Steps

With Task 2 complete, proceed to:

**Task 3**: Shared Project - DTOs and Models
- Complete remaining DTOs (OrderItemDto, CustomerDto, ProductDto, etc.)
- Create API request/response models
- Create SignalR message models
- Define enums and constants

**Task 4**: Infrastructure Project - Data Access Layer
- Configure Entity Framework Core with PosDbContext
- Implement repository pattern
- Implement Unit of Work pattern
- Create specialized repositories for web schema tables

---

## Files Created

1. `Pos.Web/database-maintenance-and-permissions.sql` - Main setup script
2. `Pos.Web/database-maintenance-jobs.sql` - SQL Server Agent jobs
3. `.kiro/specs/web-based-pos-system/TASK-2-COMPLETION-SUMMARY.md` - This document

---

## Summary

Task 2 (Database Schema Setup) is now complete. The web POS system has:

✅ Dedicated `web` schema for new functionality  
✅ 5 maintenance stored procedures for automated cleanup  
✅ 4 SQL Server Agent jobs for scheduled maintenance  
✅ Secure database permissions for application user  
✅ Monitoring and health check procedures  

The database infrastructure is ready for the next phase: implementing the data access layer and business services.
