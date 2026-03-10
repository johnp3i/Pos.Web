# Testing Checkpoint - Phases 1-6

## Overview

This checkpoint validates the core authentication and authorization system before proceeding with security hardening and optimization phases.

## Prerequisites

1. ✅ WebPosMembership database created and migrated
2. ✅ POS database accessible (for user migration)
3. ✅ Connection strings configured in appsettings.json
4. ✅ JWT secret key configured

## Testing Steps

### 1. Build and Run the API

```bash
cd Pos.Web/Pos.Web.API
dotnet build
dotnet run
```

**Expected Result**: API starts successfully on https://localhost:7001 (or configured port)

**Check for**:
- No build errors
- Database connection successful
- Roles seeded (Admin, Manager, Cashier, Waiter, Kitchen)
- Background services started (SessionCleanupService, AuditLogArchivalService)

### 2. Test Database Schema

**Verify tables exist in WebPosMembership database:**

```sql
-- Check Identity tables
SELECT * FROM AspNetRoles
SELECT * FROM AspNetUsers
SELECT * FROM AspNetUserRoles

-- Check custom tables
SELECT * FROM RefreshTokens
SELECT * FROM UserSessions
SELECT * FROM AuthAuditLog
SELECT * FROM PasswordHistory

-- Verify roles seeded
SELECT * FROM AspNetRoles WHERE IsSystemRole = 1
-- Should return 5 roles: Admin, Manager, Cashier, Waiter, Kitchen
```

### 3. Test User Migration

#### Option A: Using Console Utility

```bash
cd Pos.Web/Pos.Web.MigrationUtility
dotnet run status
```

**Expected**: Shows migration status (0 migrated if first run)

```bash
dotnet run migrate-all
```

**Expected**: 
- Migrates all active users from POS.dbo.Users
- Creates temporary password file
- Shows success/failure counts

#### Option B: Using API (requires admin user first)

First, manually create an admin user in the database or migrate one user manually.

### 4. Test Authentication Endpoints

#### 4.1 Test Login (Invalid Credentials)

```http
POST https://localhost:7001/api/membership/auth/login
Content-Type: application/json

{
  "username": "nonexistent",
  "password": "wrongpassword",
  "deviceType": "Desktop"
}
```

**Expected**: 401 Unauthorized with error message

**Verify in database**:
```sql
SELECT TOP 1 * FROM AuthAuditLog 
WHERE EventType = 'LoginFailed' 
ORDER BY Timestamp DESC
```

#### 4.2 Test Login (Valid Credentials)

After migration, use a migrated user's credentials:

```http
POST https://localhost:7001/api/membership/auth/login
Content-Type: application/json

{
  "username": "admin_username",
  "password": "temporary_password_from_migration",
  "deviceType": "Desktop"
}
```

**Expected**: 200 OK with:
- AccessToken (JWT)
- RefreshToken
- ExpiresIn (3600 seconds)
- User info with roles

**Verify in database**:
```sql
-- Check audit log
SELECT TOP 1 * FROM AuthAuditLog 
WHERE EventType = 'LoginSuccess' 
ORDER BY Timestamp DESC

-- Check session created
SELECT TOP 1 * FROM UserSessions 
WHERE EndedAt IS NULL 
ORDER BY CreatedAt DESC

-- Check refresh token stored
SELECT TOP 1 * FROM RefreshTokens 
WHERE RevokedAt IS NULL 
ORDER BY CreatedAt DESC
```

#### 4.3 Test Token Refresh

```http
POST https://localhost:7001/api/membership/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your_refresh_token_from_login"
}
```

**Expected**: 200 OK with new tokens

**Verify**: Old refresh token is revoked (RevokedAt set)

#### 4.4 Test Get Current User

```http
GET https://localhost:7001/api/membership/auth/me
Authorization: Bearer your_access_token
```

**Expected**: 200 OK with user details including roles

#### 4.5 Test Logout

```http
POST https://localhost:7001/api/membership/auth/logout
Authorization: Bearer your_access_token
```

**Expected**: 200 OK

**Verify in database**:
```sql
-- Session ended
SELECT * FROM UserSessions WHERE EndedAt IS NOT NULL

-- Audit log entry
SELECT TOP 1 * FROM AuthAuditLog 
WHERE EventType = 'Logout' 
ORDER BY Timestamp DESC
```

### 5. Test Password Management

#### 5.1 Test First-Login Password Change (No Token Required)

**IMPORTANT**: Migrated users have RequirePasswordChange = true and cannot login until they change their password. Use this endpoint for first-time password change:

```http
POST https://localhost:7001/api/membership/auth/first-login-password-change
Content-Type: application/json

{
  "username": "migrated_username",
  "currentPassword": "temporary_password_from_migration",
  "newPassword": "NewSecure123!@#",
  "deviceType": "Desktop"
}
```

**Expected**: 200 OK with:
- AccessToken (JWT)
- RefreshToken
- ExpiresIn (3600 seconds)
- User info with RequirePasswordChange = false

**What happens**:
1. Validates username and current password
2. Changes password to new password
3. Clears RequirePasswordChange flag
4. Automatically logs in the user
5. Returns tokens so user can proceed

**Verify in database**:
```sql
-- Check password changed
SELECT RequirePasswordChange, LastPasswordChangedAt 
FROM AspNetUsers 
WHERE UserName = 'migrated_username'
-- RequirePasswordChange should be 0 (false)

-- Check audit log
SELECT TOP 2 * FROM AuthAuditLog 
WHERE UserName = 'migrated_username' 
ORDER BY Timestamp DESC
-- Should show PasswordChanged and LoginSuccess events
```

#### 5.2 Test Change Password (Authenticated Users)

For users who are already logged in and want to change their password:

```http
POST https://localhost:7001/api/membership/auth/change-password
Authorization: Bearer your_access_token
Content-Type: application/json

{
  "currentPassword": "current_password",
  "newPassword": "NewSecure123!@#"
}
```

**Expected**: 200 OK

**Verify**:
- RequirePasswordChange flag cleared
- LastPasswordChangedAt updated
- Audit log entry created

#### 5.2 Test Password Reset (Admin)

```http
POST https://localhost:7001/api/membership/auth/reset-password/user_id
Authorization: Bearer admin_access_token
Content-Type: application/json

{
  "newPassword": "ResetPassword123!@#"
}
```

**Expected**: 200 OK (Admin only)

#### 5.3 Test Account Unlock (Admin)

First, lock an account by failing login 5 times, then:

```http
POST https://localhost:7001/api/membership/auth/unlock-account/user_id
Authorization: Bearer admin_access_token
```

**Expected**: 200 OK (Admin only)

### 6. Test Session Management

#### 6.1 Test Get Active Sessions

```http
GET https://localhost:7001/api/session/active
Authorization: Bearer your_access_token
```

**Expected**: List of active sessions for current user

#### 6.2 Test End Specific Session

```http
DELETE https://localhost:7001/api/session/{session_id}
Authorization: Bearer your_access_token
```

**Expected**: 200 OK, session ended

#### 6.3 Test End All Sessions

```http
DELETE https://localhost:7001/api/session/all
Authorization: Bearer your_access_token
```

**Expected**: 200 OK, all sessions ended

### 7. Test Audit Logging

#### 7.1 Test Query User Audit Logs (Admin/Manager)

```http
GET https://localhost:7001/api/audit/user/user_id?fromDate=2024-01-01&toDate=2024-12-31
Authorization: Bearer admin_access_token
```

**Expected**: List of audit logs for user

#### 7.2 Test Query Failed Logins (Admin/Manager)

```http
GET https://localhost:7001/api/audit/failed-logins?fromDate=2024-01-01
Authorization: Bearer admin_access_token
```

**Expected**: List of failed login attempts

### 8. Test Account Lockout

#### 8.1 Trigger Account Lockout

Make 5 failed login attempts with same username:

```http
POST https://localhost:7001/api/membership/auth/login
Content-Type: application/json

{
  "username": "test_user",
  "password": "wrong_password",
  "deviceType": "Desktop"
}
```

Repeat 5 times.

**Expected**: 
- First 4 attempts: 401 Unauthorized
- 5th attempt: Account locked
- 6th attempt: Error message about account lockout

**Verify in database**:
```sql
SELECT LockoutEnd, AccessFailedCount 
FROM AspNetUsers 
WHERE UserName = 'test_user'
-- LockoutEnd should be 15 minutes in future
-- AccessFailedCount should be 5
```

### 9. Test Background Services

#### 9.1 Session Cleanup Service

Wait 1 hour or manually trigger by restarting the API.

**Verify**: Sessions inactive >24 hours are ended (EndedAt set)

#### 9.2 Audit Log Archival Service

This runs monthly. For testing, you can manually check the service is registered:

```bash
# Check logs for service startup
# Should see: "AuditLogArchivalService started"
```

### 10. Test Authorization

#### 10.1 Test Admin-Only Endpoints

Try accessing admin endpoints without admin role:

```http
POST https://localhost:7001/api/migration/migrate-all
Authorization: Bearer non_admin_access_token
```

**Expected**: 403 Forbidden

#### 10.2 Test Manager/Admin Endpoints

```http
GET https://localhost:7001/api/audit/user/user_id
Authorization: Bearer cashier_access_token
```

**Expected**: 403 Forbidden (requires Admin or Manager role)

## Common Issues and Solutions

### Issue 1: Database Connection Failed

**Solution**: 
- Verify connection strings in appsettings.json
- Check SQL Server is running
- Verify Windows Authentication or credentials

### Issue 2: Roles Not Seeded

**Solution**:
- Check DbInitializer is called in Program.cs
- Manually run: `dotnet ef database update`
- Check logs for seeding errors

### Issue 3: JWT Token Validation Failed

**Solution**:
- Verify JWT secret key is configured
- Check token expiration (60 minutes)
- Verify issuer and audience match configuration

### Issue 4: Migration Fails

**Solution**:
- Verify POS database connection string
- Check dbo.Users table exists and has data
- Review migration error messages in console output

### Issue 5: Session Not Created on Login

**Solution**:
- Check SessionManager is registered in DI
- Verify UserSessions table exists
- Check logs for session creation errors

## Success Criteria

✅ All API endpoints respond correctly
✅ Authentication flow works (login, refresh, logout)
✅ Session tracking works (created, updated, ended)
✅ Audit logging captures all events
✅ Password management works (change, reset)
✅ Account lockout works after 5 failed attempts
✅ Role-based authorization works
✅ User migration completes successfully
✅ Background services run without errors

## Next Steps After Testing

If all tests pass:
1. ✅ Continue with Phase 7: Security Hardening
2. ✅ Continue with Phase 8: Performance Optimization
3. ✅ Continue with Phase 9: Error Handling and Logging

If tests fail:
1. Review error messages and logs
2. Check database state
3. Verify configuration
4. Fix issues before proceeding

## Testing Tools

### Recommended Tools:
- **Postman** or **Insomnia**: For API testing
- **SQL Server Management Studio**: For database verification
- **Swagger UI**: Available at https://localhost:7001/swagger (if configured)
- **Browser DevTools**: For inspecting JWT tokens (jwt.io)

### Sample Postman Collection

Create a Postman collection with all endpoints above for easy testing.

## Performance Baseline

Record these metrics for comparison after optimization:
- Login response time: _____ ms
- Token refresh time: _____ ms
- Token validation time: _____ ms
- Audit log write time: _____ ms

Target metrics (Phase 8):
- Login: <200ms
- Token refresh: <50ms
- Token validation: <10ms
- Audit log write: <20ms

---

**Testing Date**: _____________
**Tested By**: _____________
**Result**: ✅ Pass / ❌ Fail
**Notes**: _____________
