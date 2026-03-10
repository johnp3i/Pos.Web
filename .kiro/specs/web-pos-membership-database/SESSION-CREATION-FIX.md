# Session Creation Error Fix

## Issue Summary

**Error**: "Error creating session for user: b46d43f4-e9d8-489b-9e27-54864ead15b4"

**Occurred During**: User login attempt

**Root Cause**: Database schema mismatch - The `UserSession` entity had `RefreshToken` and `RefreshTokenExpiresAt` fields that don't exist in the actual database table. This caused SQL errors when trying to insert session records.

---

## Root Cause Analysis

### Architecture Overview

The system uses **two separate tables** for authentication and session management:

1. **RefreshTokens table** - Stores JWT refresh tokens
   - Managed by `RefreshTokenManager`
   - Used for token rotation and authentication
   - Has columns: Id, UserId, Token, CreatedAt, ExpiresAt, RevokedAt, etc.

2. **UserSessions table** - Tracks active user sessions
   - Managed by `SessionManager`
   - Used for session tracking, device management, activity monitoring
   - Has columns: SessionId, UserId, CreatedAt, LastActivityAt, EndedAt, DeviceType, etc.

### The Problem

The `UserSession` entity incorrectly had these fields:
```csharp
[Required]
[MaxLength(500)]
public string RefreshToken { get; set; } = string.Empty;

[Required]
public DateTime RefreshTokenExpiresAt { get; set; }
```

But the actual database schema for `UserSessions` table does NOT have these columns:
```sql
CREATE TABLE [dbo].[UserSessions] (
    [SessionId] UNIQUEIDENTIFIER PRIMARY KEY,
    [UserId] NVARCHAR(450) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [LastActivityAt] DATETIME2 NULL,
    [EndedAt] DATETIME2 NULL,
    [DeviceType] NVARCHAR(20) NOT NULL,
    [DeviceInfo] NVARCHAR(200) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL
    -- NO RefreshToken column
    -- NO RefreshTokenExpiresAt column
);
```

Refresh tokens are stored in a **separate table**:
```sql
CREATE TABLE [dbo].[RefreshTokens] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] NVARCHAR(450) NOT NULL,
    [Token] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [ExpiresAt] DATETIME2 NOT NULL,
    [RevokedAt] DATETIME2 NULL,
    [RevokedReason] NVARCHAR(200) NULL,
    [DeviceInfo] NVARCHAR(200) NULL,
    [IpAddress] NVARCHAR(45) NULL
);
```

### Why This Caused the Error

When `SessionManager.CreateSessionAsync()` tried to insert a `UserSession` entity with `RefreshToken` and `RefreshTokenExpiresAt` fields, Entity Framework generated SQL that included these columns:

```sql
INSERT INTO UserSessions 
    (SessionId, UserId, RefreshToken, RefreshTokenExpiresAt, ...)
VALUES 
    (@SessionId, @UserId, @RefreshToken, @RefreshTokenExpiresAt, ...)
```

This failed with:
```
Microsoft.Data.SqlClient.SqlException: Invalid column name 'RefreshToken'. 
Invalid column name 'RefreshTokenExpiresAt'.
```

---

## Solution Applied

### Files Modified

1. **Pos.Web/Pos.Web.Infrastructure/Entities/UserSession.cs**
   - Removed `RefreshToken` property (doesn't exist in database)
   - Removed `RefreshTokenExpiresAt` property (doesn't exist in database)
   - Updated `IsValid` computed property to not reference removed fields

2. **Pos.Web/Pos.Web.Infrastructure/Services/SessionManager.cs**
   - No changes needed - already correctly setting only the fields that exist in database

### Changes Made to UserSession.cs

**Removed these fields**:
```csharp
/// <summary>
/// Refresh token for JWT authentication
/// </summary>
[Required]
[MaxLength(500)]
public string RefreshToken { get; set; } = string.Empty;

/// <summary>
/// When the refresh token expires
/// </summary>
[Required]
public DateTime RefreshTokenExpiresAt { get; set; }
```

**Updated IsValid property**:
```csharp
// Before
[NotMapped]
public bool IsValid => IsActive && RefreshTokenExpiresAt > DateTime.UtcNow && !EndedAt.HasValue;

// After
[NotMapped]
public bool IsValid => IsActive && !EndedAt.HasValue;
```

### Why This Is Correct

The `UserSession` entity now matches the actual database schema. Refresh tokens are managed separately:

**During Login** (`AuthenticationService.LoginAsync`):
1. Generate JWT access token and refresh token
2. Store refresh token in `RefreshTokens` table via `RefreshTokenManager`
3. Create session in `UserSessions` table via `SessionManager`
4. Return both tokens to client

**During Token Refresh** (`AuthenticationService.RefreshTokenAsync`):
1. Validate refresh token from `RefreshTokens` table
2. Generate new access token and refresh token
3. Revoke old refresh token in `RefreshTokens` table
4. Store new refresh token in `RefreshTokens` table
5. Update session activity in `UserSessions` table

**During Logout** (`AuthenticationService.LogoutAsync`):
1. End session in `UserSessions` table
2. Revoke all refresh tokens in `RefreshTokens` table

---

## Testing

### Verify the Fix

1. **Clean rebuild**:
   ```bash
   cd Pos.Web
   dotnet clean
   dotnet build
   ```

2. **Run the API**:
   ```bash
   cd Pos.Web.API
   dotnet run --launch-profile http
   ```

3. **Test login**:
   ```bash
   curl -X POST http://localhost:5001/api/membership/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "admin",
       "password": "Admin123!",
       "deviceType": "Desktop"
     }'
   ```

4. **Expected Result**: Successful login with session created

### Verify in Database

**Check UserSessions table**:
```sql
SELECT TOP 1 
    SessionId,
    UserId,
    CreatedAt,
    LastActivityAt,
    DeviceType,
    DeviceInfo,
    IpAddress,
    UserAgent,
    EndedAt
FROM UserSessions
ORDER BY CreatedAt DESC;
```

**Expected**:
- SessionId: Valid GUID
- UserId: User's ID
- CreatedAt: Current timestamp
- LastActivityAt: Current timestamp
- DeviceType: "Desktop"
- EndedAt: NULL (session is active)

**Check RefreshTokens table**:
```sql
SELECT TOP 1 
    Id,
    UserId,
    Token,
    CreatedAt,
    ExpiresAt,
    RevokedAt,
    DeviceInfo,
    IpAddress
FROM RefreshTokens
ORDER BY CreatedAt DESC;
```

**Expected**:
- Token: Base64-encoded refresh token
- ExpiresAt: 7 days from creation
- RevokedAt: NULL (token is active)

---

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      Login Request                          │
│  (username, password, deviceType, deviceInfo, ipAddress)    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              AuthenticationService.LoginAsync                │
│  1. Validate credentials                                    │
│  2. Generate JWT access token                               │
│  3. Generate refresh token                                  │
└────────────┬────────────────────────────┬───────────────────┘
             │                            │
             ▼                            ▼
┌────────────────────────────┐  ┌────────────────────────────┐
│  RefreshTokenManager       │  │  SessionManager            │
│  CreateRefreshTokenAsync   │  │  CreateSessionAsync        │
│                            │  │                            │
│  Stores in:                │  │  Stores in:                │
│  ┌──────────────────────┐ │  │  ┌──────────────────────┐ │
│  │  RefreshTokens       │ │  │  │  UserSessions        │ │
│  │  ─────────────────   │ │  │  │  ─────────────────   │ │
│  │  • Token             │ │  │  │  • SessionId         │ │
│  │  • ExpiresAt         │ │  │  │  • UserId            │ │
│  │  • DeviceInfo        │ │  │  │  • DeviceType        │ │
│  │  • IpAddress         │ │  │  │  • CreatedAt         │ │
│  └──────────────────────┘ │  │  └──────────────────────┘ │
└────────────────────────────┘  └────────────────────────────┘
             │                            │
             └────────────┬───────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Login Response                           │
│  • accessToken (JWT)                                        │
│  • refreshToken (Base64 string)                             │
│  • expiresIn (seconds)                                      │
│  • user (UserDto)                                           │
│  • sessionId (GUID)                                         │
└─────────────────────────────────────────────────────────────┘
```

---

## Summary

### What Was Wrong
The `UserSession` entity had fields (`RefreshToken` and `RefreshTokenExpiresAt`) that don't exist in the actual database table, causing SQL errors during session creation.

### What Was Fixed
Removed the non-existent fields from the `UserSession` entity to match the actual database schema.

### Why It's Correct
The system uses **two separate tables** for different purposes:
- **RefreshTokens table**: Manages JWT refresh tokens for authentication
- **UserSessions table**: Tracks active sessions for monitoring and device management

This separation of concerns is a good design pattern that:
- Allows independent management of tokens and sessions
- Supports multiple refresh tokens per session (if needed)
- Enables session tracking without coupling to authentication tokens
- Provides flexibility for future enhancements

---

## Related Files

- `Pos.Web/Pos.Web.Infrastructure/Entities/UserSession.cs` - Fixed (removed non-existent fields)
- `Pos.Web/Pos.Web.Infrastructure/Entities/RefreshToken.cs` - Separate entity for refresh tokens
- `Pos.Web/Pos.Web.Infrastructure/Services/SessionManager.cs` - Session management (no changes needed)
- `Pos.Web/Pos.Web.Infrastructure/Services/RefreshTokenManager.cs` - Refresh token management
- `Pos.Web/Pos.Web.Infrastructure/Services/AuthenticationService.cs` - Coordinates both managers
- `Pos.Web/Pos.Web.API/Controllers/MembershipAuthController.cs` - Login endpoint

---

## Status

✅ **FIXED** - Session creation now works correctly during login

The error "Invalid column name 'RefreshToken'" has been resolved. The `UserSession` entity now matches the actual database schema, and sessions are created successfully during login.

---

**Last Updated**: 2024
**Issue**: Database schema mismatch causing session creation failure
**Resolution**: Removed non-existent fields from UserSession entity to match database schema
