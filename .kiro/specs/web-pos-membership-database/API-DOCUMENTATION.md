# Web POS Membership API Documentation

**Version:** 1.0.0  
**Base URL:** `https://api.yourcompany.com` (Production) | `https://localhost:7001` (Development)  
**Authentication:** JWT Bearer Token

---

## Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [Session Management Endpoints](#session-management-endpoints)
3. [Audit Log Endpoints](#audit-log-endpoints)
4. [User Migration Endpoints](#user-migration-endpoints)
5. [Error Codes](#error-codes)
6. [Authentication Flow](#authentication-flow)
7. [Rate Limiting](#rate-limiting)

---

## Authentication Endpoints

### POST /api/membership/auth/login

Authenticates a user and returns access and refresh tokens.

**Request Body:**
```json
{
  "username": "string",
  "password": "string",
  "deviceType": "Desktop|Tablet|Mobile",
  "rememberMe": false
}
```

**Success Response (200 OK):**
```json
{
  "isSuccessful": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-token",
  "expiresIn": 3600,
  "sessionId": "guid",
  "user": {
    "id": "string",
    "userName": "string",
    "email": "string",
    "displayName": "string",
    "employeeId": 123,
    "roles": ["Admin", "Manager"]
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "isSuccessful": false,
  "errorMessage": "Invalid username or password",
  "errorCode": 1001
}
```

**Error Response (403 Forbidden - Account Locked):**
```json
{
  "isSuccessful": false,
  "errorMessage": "Account is locked due to multiple failed login attempts",
  "errorCode": 1003,
  "lockoutEnd": "2026-03-04T15:30:00Z"
}
```

**Rate Limit:** 100 requests per minute per IP

---

### POST /api/membership/auth/refresh

Refreshes an expired access token using a refresh token.

**Request Body:**
```json
{
  "refreshToken": "base64-encoded-token"
}
```

**Success Response (200 OK):**
```json
{
  "isSuccessful": true,
  "accessToken": "new-jwt-token",
  "refreshToken": "new-refresh-token",
  "expiresIn": 3600
}
```

**Error Response (401 Unauthorized):**
```json
{
  "isSuccessful": false,
  "errorMessage": "Invalid or expired refresh token",
  "errorCode": 1002
}
```

**Rate Limit:** 1000 requests per minute per IP

---

### POST /api/membership/auth/logout

Logs out the current user, ending their session and revoking tokens.

**Authentication:** Required (Bearer Token)

**Request Body:** None

**Success Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "errorCode": 1000,
  "message": "Authentication required",
  "correlationId": "guid"
}
```

---

### POST /api/membership/auth/change-password

Changes the password for the authenticated user.

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least 1 digit
- At least 1 lowercase letter
- At least 1 uppercase letter
- At least 1 non-alphanumeric character
- At least 4 unique characters
- Cannot reuse last 5 passwords

**Success Response (200 OK):**
```json
{
  "message": "Password changed successfully. All sessions have been terminated."
}
```

**Error Response (400 Bad Request):**
```json
{
  "errorCode": 2001,
  "message": "Password does not meet complexity requirements",
  "details": "Password must contain at least one uppercase letter",
  "correlationId": "guid"
}
```

---

### POST /api/membership/auth/reset-password/{userId}

Resets a user's password (Admin only).

**Authentication:** Required (Admin role)

**Path Parameters:**
- `userId` (string): The ID of the user whose password to reset

**Request Body:**
```json
{
  "newPassword": "string"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Password reset successfully. User will be required to change password on next login."
}
```

**Error Response (403 Forbidden):**
```json
{
  "errorCode": 3001,
  "message": "You do not have permission to perform this operation",
  "correlationId": "guid"
}
```

---

### POST /api/membership/auth/unlock-account/{userId}

Unlocks a locked user account (Admin only).

**Authentication:** Required (Admin role)

**Path Parameters:**
- `userId` (string): The ID of the user to unlock

**Success Response (200 OK):**
```json
{
  "message": "Account unlocked successfully"
}
```

---

### GET /api/membership/auth/me

Gets the current authenticated user's information.

**Authentication:** Required (Bearer Token)

**Success Response (200 OK):**
```json
{
  "id": "string",
  "userName": "string",
  "email": "string",
  "displayName": "string",
  "employeeId": 123,
  "roles": ["Admin", "Manager"],
  "isActive": true,
  "lastLoginAt": "2026-03-04T10:00:00Z",
  "requirePasswordChange": false
}
```

---

## Session Management Endpoints

### GET /api/sessions/active

Gets all active sessions for the current user.

**Authentication:** Required (Bearer Token)

**Success Response (200 OK):**
```json
{
  "sessions": [
    {
      "sessionId": "guid",
      "deviceType": "Desktop",
      "deviceInfo": "Windows 11, Chrome 120",
      "ipAddress": "192.168.1.100",
      "createdAt": "2026-03-04T09:00:00Z",
      "lastActivityAt": "2026-03-04T10:30:00Z"
    }
  ]
}
```

---

### DELETE /api/sessions/{sessionId}

Ends a specific session.

**Authentication:** Required (Bearer Token)

**Path Parameters:**
- `sessionId` (guid): The session ID to end

**Success Response (200 OK):**
```json
{
  "message": "Session ended successfully"
}
```

---

### DELETE /api/sessions/user/{userId}/all

Ends all sessions for a specific user (Admin only).

**Authentication:** Required (Admin role)

**Path Parameters:**
- `userId` (string): The user ID whose sessions to end

**Success Response (200 OK):**
```json
{
  "message": "All user sessions ended",
  "sessionsEnded": 3
}
```

---

## Audit Log Endpoints

### GET /api/audit/user/{userId}

Gets audit logs for a specific user (Admin/Manager only).

**Authentication:** Required (Admin or Manager role)

**Path Parameters:**
- `userId` (string): The user ID to query

**Query Parameters:**
- `fromDate` (datetime, optional): Start date for log query
- `toDate` (datetime, optional): End date for log query
- `eventType` (string, optional): Filter by event type

**Success Response (200 OK):**
```json
{
  "logs": [
    {
      "id": 1,
      "userId": "string",
      "userName": "john.doe",
      "eventType": "LoginSuccess",
      "timestamp": "2026-03-04T10:00:00Z",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0...",
      "details": "Login successful",
      "isSuccessful": true
    }
  ],
  "totalCount": 150,
  "pageSize": 50,
  "currentPage": 1
}
```

---

### GET /api/audit/failed-logins

Gets all failed login attempts (Admin/Manager only).

**Authentication:** Required (Admin or Manager role)

**Query Parameters:**
- `fromDate` (datetime, optional): Start date
- `toDate` (datetime, optional): End date
- `pageSize` (int, optional): Number of records per page (default: 50)
- `page` (int, optional): Page number (default: 1)

**Success Response (200 OK):**
```json
{
  "logs": [
    {
      "id": 1,
      "userName": "john.doe",
      "eventType": "LoginFailed",
      "timestamp": "2026-03-04T10:00:00Z",
      "ipAddress": "192.168.1.100",
      "errorMessage": "Invalid password",
      "isSuccessful": false
    }
  ],
  "totalCount": 25
}
```

---

## User Migration Endpoints

### POST /api/migration/migrate-all

Migrates all active users from the legacy POS system (Admin only).

**Authentication:** Required (Admin role)

**Request Body:**
```json
{
  "forcePasswordReset": true
}
```

**Success Response (200 OK):**
```json
{
  "totalUsers": 50,
  "successfulMigrations": 48,
  "failedMigrations": 2,
  "errors": [
    {
      "legacyUserId": 123,
      "userName": "john.doe",
      "errorMessage": "User already exists"
    }
  ],
  "duration": "00:00:15"
}
```

---

### POST /api/migration/migrate-user/{legacyUserId}

Migrates a single user from the legacy system (Admin only).

**Authentication:** Required (Admin role)

**Path Parameters:**
- `legacyUserId` (int): The legacy user ID to migrate

**Request Body:**
```json
{
  "temporaryPassword": "TempPass123!"
}
```

**Success Response (200 OK):**
```json
{
  "message": "User migrated successfully",
  "userId": "new-identity-user-id",
  "temporaryPassword": "TempPass123!"
}
```

---

### GET /api/migration/status

Gets the migration status report (Admin only).

**Authentication:** Required (Admin role)

**Success Response (200 OK):**
```json
{
  "totalLegacyUsers": 50,
  "migratedUsers": 48,
  "pendingUsers": 2,
  "lastMigrationDate": "2026-03-04T10:00:00Z"
}
```

---

## Error Codes

| Code | Description |
|------|-------------|
| 1000 | Unauthorized - Authentication required |
| 1001 | Invalid Credentials - Username or password incorrect |
| 1002 | Invalid Token - Token is invalid or expired |
| 1003 | Account Locked - Too many failed login attempts |
| 1004 | Account Inactive - User account is deactivated |
| 1005 | Password Change Required - User must change password |
| 2000 | Validation Error - Request validation failed |
| 2001 | Invalid Password - Password doesn't meet requirements |
| 2002 | Password Reuse - Cannot reuse recent passwords |
| 3000 | Database Error - Database operation failed |
| 3001 | Forbidden - Insufficient permissions |
| 4000 | Internal Server Error - Unexpected error occurred |
| 5000 | Service Unavailable - Database connection failed |
| 6000 | Migration Failed - User migration error |

---

## Authentication Flow

### Standard Login Flow

```
1. Client → POST /api/membership/auth/login
   ↓
2. API validates credentials
   ↓
3. API creates session
   ↓
4. API generates access token (60 min) and refresh token (7 days)
   ↓
5. API returns tokens + user info
   ↓
6. Client stores tokens in local storage
   ↓
7. Client includes access token in Authorization header for all requests
```

### Token Refresh Flow

```
1. Client detects token expiring soon (5 min before expiration)
   ↓
2. Client → POST /api/membership/auth/refresh
   ↓
3. API validates refresh token
   ↓
4. API generates new access token and refresh token
   ↓
5. API revokes old refresh token
   ↓
6. API returns new tokens
   ↓
7. Client updates stored tokens
```

### Logout Flow

```
1. Client → POST /api/membership/auth/logout
   ↓
2. API ends user session
   ↓
3. API revokes all refresh tokens for session
   ↓
4. Client clears stored tokens
   ↓
5. Client redirects to login page
```

---

## Rate Limiting

Rate limits are enforced per IP address:

| Endpoint | Limit |
|----------|-------|
| `/api/membership/auth/login` | 100 requests/minute |
| `/api/membership/auth/refresh` | 1000 requests/minute |
| All other endpoints | 1000 requests/minute |

**Rate Limit Headers:**
```
X-Rate-Limit-Limit: 100
X-Rate-Limit-Remaining: 95
X-Rate-Limit-Reset: 1709557200
```

**Rate Limit Exceeded Response (429 Too Many Requests):**
```json
{
  "errorCode": 4001,
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 60
}
```

---

## Request/Response Headers

### Required Request Headers

```
Authorization: Bearer {access-token}
Content-Type: application/json
```

### Response Headers

```
Content-Type: application/json
X-Correlation-Id: {guid}
X-Rate-Limit-Limit: 1000
X-Rate-Limit-Remaining: 995
```

---

## Swagger/OpenAPI

Interactive API documentation is available at:

- **Development:** `https://localhost:7001/swagger`
- **Production:** `https://api.yourcompany.com/swagger`

The Swagger UI provides:
- Interactive API testing
- Request/response examples
- Schema definitions
- Authentication testing

---

## Security Considerations

1. **HTTPS Only:** All API endpoints require HTTPS in production
2. **Token Storage:** Store tokens securely (HttpOnly cookies or secure local storage)
3. **Token Expiration:** Access tokens expire after 60 minutes
4. **Refresh Token Rotation:** Refresh tokens are rotated on each use
5. **Account Lockout:** Accounts lock after 5 failed login attempts for 15 minutes
6. **Password Policy:** Enforced complexity requirements and history checking
7. **Audit Logging:** All authentication events are logged
8. **CORS:** Configured with explicit origin whitelist
9. **Rate Limiting:** Prevents brute force attacks

---

## Support

For API support, contact:
- **Email:** support@yourcompany.com
- **Documentation:** https://docs.yourcompany.com
- **Status Page:** https://status.yourcompany.com
