# Phase 2 Complete: Database Schema Setup

**Date**: 2026-03-05  
**Status**: ✅ All Tasks Complete

---

## Summary

Phase 2 (Database Schema Setup) is now fully complete with all column name mismatches corrected. The maintenance scripts now accurately reference the actual database schema.

---

## What Was Fixed

### Column Name Corrections

The maintenance stored procedures were updated to match the actual schema in `database-scripts.sql`:

#### 1. web.OrderLocks Table
**Corrected Columns**:
- `LockAcquiredAt` (was incorrectly referenced as `LockedAt`)
- `LockExpiresAt` (was incorrectly referenced as `ExpiresAt`)
- `IsActive` (now properly used)
- `UpdatedAt` (now properly used)

**Removed Incorrect References**:
- `LockedBy`, `LockedByUserName`, `SessionID`, `DeviceInfo` (these don't exist in the actual schema)

#### 2. web.ApiAuditLog Table
**Correct Columns Used**:
- `Timestamp`, `Action`, `EntityType`, `EntityID`
- `RequestMethod`, `Duration`
- `OldValues`, `NewValues`

**Removed Incorrect References**:
- `UserName`, `Changes`, `HttpMethod` (these don't exist in the actual schema)

#### 3. web.UserSessions Table
**Corrected Columns**:
- `RefreshTokenExpiresAt` (was incorrectly referenced as `ExpiresAt`)
- `LoggedOutAt` (was incorrectly referenced as `EndedAt`)
- `SessionID`, `IsActive`, `CreatedAt`, `LastActivityAt` (now properly used)

**Removed Incorrect References**:
- `UserName`, `DeviceType`, `DeviceName`, `BrowserName`, `BrowserVersion` (these don't exist in the actual schema)

#### 4. web.FeatureFlags Table
**Corrected Columns**:
- `Name` (was incorrectly referenced as `FeatureName`)
- `UpdatedBy` (now properly used)

**Removed Incorrect References**:
- `DisplayName`, `EnabledPercentage`, `CreatedBy` (these don't exist in the actual schema)

#### 5. web.SyncQueue Table
**Corrected Columns**:
- `ClientTimestamp` (was incorrectly referenced as `QueuedAt`)
- `AttemptCount` (was incorrectly referenced as `RetryCount`)
- `Status`, `ProcessedAt`, `LastAttemptAt` (now properly used)

**Removed Incorrect References**:
- `MaxRetries`, `LastError` (these don't exist in the actual schema)

---

## Updated Stored Procedures

### 1. web.CleanupExpiredLocks
**Changes**:
- Now uses `LockExpiresAt` instead of `ExpiresAt`
- Now uses `IsActive` flag to deactivate locks
- Now uses `UpdatedAt` for tracking deactivation time
- Deletes inactive locks older than 7 days

**Behavior**:
```sql
-- Deactivate expired locks
UPDATE web.OrderLocks
SET IsActive = 0, UpdatedAt = GETUTCDATE()
WHERE IsActive = 1 AND LockExpiresAt < GETUTCDATE();

-- Delete old inactive locks
DELETE FROM web.OrderLocks
WHERE IsActive = 0 AND UpdatedAt < DATEADD(DAY, -7, GETUTCDATE());
```

### 2. web.CleanupExpiredSessions
**Changes**:
- Now uses `RefreshTokenExpiresAt` instead of `ExpiresAt`
- Now sets `LoggedOutAt` when deactivating sessions
- Deletes inactive sessions older than 30 days

**Behavior**:
```sql
-- Mark expired sessions as inactive
UPDATE web.UserSessions
SET IsActive = 0, LoggedOutAt = GETUTCDATE()
WHERE RefreshTokenExpiresAt < GETUTCDATE() AND IsActive = 1;

-- Delete old inactive sessions
DELETE FROM web.UserSessions
WHERE IsActive = 0 AND LoggedOutAt < DATEADD(DAY, -30, GETUTCDATE());
```

### 3. web.CleanupFailedSyncQueue
**Changes**:
- Now uses `ClientTimestamp` instead of `QueuedAt`
- Now uses `AttemptCount` instead of `RetryCount`
- Added `@MaxAttempts` parameter (default: 5)
- Removed reference to non-existent `MaxRetries` column

**Behavior**:
```sql
DELETE FROM web.SyncQueue
WHERE Status = 'Failed'
  AND ClientTimestamp < @CutoffDate
  AND AttemptCount >= @MaxAttempts;
```

### 4. web.GetMaintenanceStatus
**Changes**:
- OrderLocks: Now uses `LockAcquiredAt`, `LockExpiresAt`, `IsActive`
- UserSessions: Columns already correct
- ApiAuditLog: Columns already correct
- FeatureFlags: Columns already correct
- SyncQueue: Now uses `ClientTimestamp` instead of `QueuedAt`

---

## Updated SQL Agent Jobs

### Job: WebPOS - Cleanup Failed Sync Queue
**Updated Command**:
```sql
EXEC [POS].[web].[CleanupFailedSyncQueue] @RetentionDays = 7, @MaxAttempts = 5;
```

**Previous Command** (incorrect):
```sql
EXEC [POS].[web].[CleanupFailedSyncQueue] @RetentionDays = 7;
```

---

## Files Updated

1. ✅ `Pos.Web/database-maintenance-and-permissions.sql`
   - Fixed all stored procedure column references
   - Updated procedure logic to match actual schema

2. ✅ `Pos.Web/database-maintenance-jobs.sql`
   - Updated job step command for CleanupFailedSyncQueue
   - Added documentation for modifying max attempts parameter

3. ✅ `.kiro/specs/web-based-pos-system/TASK-2-COMPLETION-SUMMARY.md`
   - Updated procedure documentation with correct column names
   - Added column definitions section
   - Updated test procedures with correct parameters

4. ✅ `.kiro/specs/web-based-pos-system/tasks.md`
   - Marked Task 2.2 as completed
   - Marked Task 2.3 as completed

---

## How to Deploy

### Step 1: Run Maintenance and Permissions Script
```sql
-- Connect to POS database in SQL Server Management Studio
-- Open: Pos.Web/database-maintenance-and-permissions.sql
-- Execute the entire script (F5)
```

**Expected Output**:
```
Created: web.CleanupExpiredLocks
Created: web.CleanupOldAuditLogs
Created: web.CleanupExpiredSessions
Created: web.CleanupFailedSyncQueue
Created: web.GetMaintenanceStatus
All maintenance stored procedures created successfully!

Configuring Database Permissions
Created database user: WebPosAppUser
  - Granted SELECT on dbo schema
  - Granted INSERT, UPDATE on Invoices and InvoiceItems
  - Granted INSERT, UPDATE on PendingInvoices and PendingInvoiceItems
  - Granted INSERT, UPDATE on Customers and CustomerAddresses
  - Granted INSERT on ServerCommandsHistory
  - Granted SELECT, INSERT, UPDATE, DELETE on web schema
  - Granted EXECUTE on web schema stored procedures
```

### Step 2: Run SQL Agent Jobs Script (Optional)
```sql
-- Connect to msdb database in SQL Server Management Studio
-- Open: Pos.Web/database-maintenance-jobs.sql
-- Execute the entire script (F5)
```

**Expected Output**:
```
Created job: WebPOS - Cleanup Expired Locks (every 5 minutes)
Created job: WebPOS - Cleanup Expired Sessions (every hour)
Created job: WebPOS - Cleanup Old Audit Logs (daily at 2:00 AM)
Created job: WebPOS - Cleanup Failed Sync Queue (daily at 3:00 AM)
SQL Server Agent Jobs Created Successfully!
```

### Step 3: Test Procedures
```sql
-- Test each procedure manually
EXEC web.CleanupExpiredLocks;
EXEC web.CleanupExpiredSessions;
EXEC web.CleanupOldAuditLogs @RetentionDays = 90;
EXEC web.CleanupFailedSyncQueue @RetentionDays = 7, @MaxAttempts = 5;

-- Check maintenance status
EXEC web.GetMaintenanceStatus;
```

---

## Verification Checklist

- [x] All stored procedures reference correct column names
- [x] web.CleanupExpiredLocks uses `LockExpiresAt`, `IsActive`, `UpdatedAt`
- [x] web.CleanupExpiredSessions uses `RefreshTokenExpiresAt`, `LoggedOutAt`
- [x] web.CleanupFailedSyncQueue uses `ClientTimestamp`, `AttemptCount`, `@MaxAttempts`
- [x] web.GetMaintenanceStatus uses correct columns for all tables
- [x] SQL Agent job commands updated with correct parameters
- [x] Documentation updated with correct column names
- [x] Tasks 2.2 and 2.3 marked as completed

---

## Next Steps

With Phase 2 complete, you can now proceed to:

**Task 3: Shared Project - DTOs and Models**
- Create core domain models (OrderDto, CustomerDto, ProductDto)
- Create API request/response models
- Create SignalR message models
- Define enums and constants

**Task 4: Infrastructure Project - Data Access Layer**
- Configure Entity Framework Core with PosDbContext
- Implement repository pattern
- Implement Unit of Work pattern
- Create specialized repositories for web schema tables

---

## Summary

Phase 2 is now fully complete with all column name mismatches resolved. The maintenance scripts accurately reflect the actual database schema defined in `database-scripts.sql`. You can now safely run these scripts in your database environment.

**Key Achievement**: The web schema infrastructure is production-ready with automated maintenance, proper security, and accurate column references throughout all stored procedures.
