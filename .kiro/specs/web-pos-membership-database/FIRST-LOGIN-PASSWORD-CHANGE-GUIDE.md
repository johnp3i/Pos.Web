# First Login Password Change - User Guide

## Overview

The WebPosMembership system includes a mandatory password change feature for users logging in for the first time or when their password has been reset by an administrator. This guide explains how to use the first login password change endpoint.

---

## When Is Password Change Required?

A user is required to change their password when:
1. **First login** - New user accounts created by administrators
2. **Password reset** - Administrator resets a user's password
3. **Security policy** - Account flagged for password change due to security concerns

The system tracks this requirement using the `RequirePasswordChange` flag in the `AspNetUsers` table.

---

## API Endpoint

### Endpoint Details
- **URL**: `/api/membership/auth/first-login-password-change`
- **Method**: `POST`
- **Authentication**: Not required (user hasn't logged in yet)
- **Content-Type**: `application/json`

### Request Body

```json
{
  "username": "string",
  "currentPassword": "string",
  "newPassword": "string"
}
```

**Field Descriptions**:
- `username` (required): The user's username
- `currentPassword` (required): The temporary/current password
- `newPassword` (required): The new password that meets security requirements

---

## Password Requirements

The new password must meet the following criteria:

| Requirement | Description |
|-------------|-------------|
| **Minimum Length** | At least 8 characters |
| **Uppercase Letter** | At least one uppercase letter (A-Z) |
| **Lowercase Letter** | At least one lowercase letter (a-z) |
| **Digit** | At least one numeric digit (0-9) |
| **Special Character** | At least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?) |
| **Password History** | Cannot reuse any of the last 5 passwords |

---

## Usage Examples

### Example 1: Using cURL

```bash
curl -X POST http://localhost:5001/api/membership/auth/first-login-password-change \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "currentPassword": "TempPassword123!",
    "newPassword": "NewSecurePass123!"
  }'
```

### Example 2: Using PowerShell

```powershell
$body = @{
    username = "admin"
    currentPassword = "TempPassword123!"
    newPassword = "NewSecurePass123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/membership/auth/first-login-password-change" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Example 3: Using Postman

1. **Method**: POST
2. **URL**: `http://localhost:5001/api/membership/auth/first-login-password-change`
3. **Headers**:
   - `Content-Type: application/json`
4. **Body** (raw JSON):
```json
{
  "username": "admin",
  "currentPassword": "TempPassword123!",
  "newPassword": "NewSecurePass123!"
}
```

### Example 4: Using JavaScript/Fetch

```javascript
const response = await fetch('http://localhost:5001/api/membership/auth/first-login-password-change', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'admin',
    currentPassword: 'TempPassword123!',
    newPassword: 'NewSecurePass123!'
  })
});

const result = await response.json();
console.log(result);
```

---

## Response Format

### Success Response (200 OK)

```json
{
  "isSuccessful": true,
  "message": "Password changed successfully. Please login with your new password.",
  "accessToken": null,
  "refreshToken": null,
  "user": null,
  "errorMessage": null
}
```

**After Success**:
- The `RequirePasswordChange` flag is set to `false`
- User can now login normally with the new password
- Password change is logged in audit logs

### Failure Responses

#### 400 Bad Request - Invalid Password

```json
{
  "isSuccessful": false,
  "message": null,
  "accessToken": null,
  "refreshToken": null,
  "user": null,
  "errorMessage": "Password does not meet security requirements"
}
```

**Common Reasons**:
- Password too short (< 8 characters)
- Missing required character types
- Password was used recently (in last 5 passwords)

#### 400 Bad Request - Invalid Current Password

```json
{
  "isSuccessful": false,
  "message": null,
  "accessToken": null,
  "refreshToken": null,
  "user": null,
  "errorMessage": "Current password is incorrect"
}
```

#### 404 Not Found - User Not Found

```json
{
  "isSuccessful": false,
  "message": null,
  "accessToken": null,
  "refreshToken": null,
  "user": null,
  "errorMessage": "User not found"
}
```

#### 400 Bad Request - Password Change Not Required

```json
{
  "isSuccessful": false,
  "message": null,
  "accessToken": null,
  "refreshToken": null,
  "user": null,
  "errorMessage": "Password change is not required for this user"
}
```

---

## User Flow

### Step-by-Step Process

1. **User attempts to login** with temporary password
2. **System detects** `RequirePasswordChange = true`
3. **Login response** indicates password change required:
   ```json
   {
     "isSuccessful": false,
     "errorMessage": "Password change required. Please change your password before logging in.",
     "requirePasswordChange": true
   }
   ```
4. **User is redirected** to password change page/form
5. **User submits** new password via `/first-login-password-change` endpoint
6. **System validates** new password against requirements
7. **System checks** password history (last 5 passwords)
8. **Password updated** and `RequirePasswordChange` set to `false`
9. **User can now login** with new password

---

## Testing & Development

### Reset User to Require Password Change

For testing purposes, you can manually set a user to require password change:

```sql
-- Set user to require password change
UPDATE AspNetUsers 
SET RequirePasswordChange = 1 
WHERE UserName = 'admin';
```

### Check Password Change Status

```sql
-- Check if user requires password change
SELECT 
    UserName,
    RequirePasswordChange,
    LastPasswordChangeDate
FROM AspNetUsers 
WHERE UserName = 'admin';
```

### View Password History

```sql
-- View user's password history
SELECT 
    ph.UserId,
    u.UserName,
    ph.PasswordHash,
    ph.CreatedAt
FROM PasswordHistory ph
INNER JOIN AspNetUsers u ON ph.UserId = u.Id
WHERE u.UserName = 'admin'
ORDER BY ph.CreatedAt DESC;
```

---

## Security Features

### Password History Tracking

The system maintains a history of the last 5 passwords for each user:
- Stored in `PasswordHistory` table
- Hashed using the same algorithm as current passwords
- Prevents password reuse
- Automatically cleaned up (keeps only last 5)

### Audit Logging

All password change attempts are logged:
- **Success**: Logged with user ID and timestamp
- **Failure**: Logged with reason for failure
- **IP Address**: Captured for security monitoring
- **Device Info**: Recorded for tracking

### Password Validation

Validation is performed at multiple levels:
1. **Client-side** (if implemented): Immediate feedback
2. **DTO Validation**: FluentValidation rules
3. **ASP.NET Identity**: Built-in password validators
4. **Custom Validation**: Password history check

---

## Implementation Details

### Controller Method

Located in: `Pos.Web.API/Controllers/MembershipAuthController.cs`

```csharp
[HttpPost("first-login-password-change")]
[AllowAnonymous]
public async Task<ActionResult<AuthenticationResultDto>> FirstLoginPasswordChange(
    [FromBody] FirstLoginPasswordChangeRequestDto request)
{
    // Implementation handles:
    // 1. User lookup
    // 2. Current password verification
    // 3. New password validation
    // 4. Password history check
    // 5. Password update
    // 6. RequirePasswordChange flag update
    // 7. Audit logging
}
```

### DTO Definition

Located in: `Pos.Web.Shared/DTOs/Authentication/FirstLoginPasswordChangeRequestDto.cs`

```csharp
public class FirstLoginPasswordChangeRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
```

### Validation Rules

Located in: `Pos.Web.API/Validators/FirstLoginPasswordChangeRequestDtoValidator.cs`

- Username: Required, 3-50 characters
- CurrentPassword: Required
- NewPassword: Required, meets password policy

---

## Troubleshooting

### Issue: "Password does not meet requirements"

**Solution**: Ensure new password includes:
- At least 8 characters
- One uppercase letter
- One lowercase letter
- One digit
- One special character

**Example Valid Password**: `SecurePass123!`

### Issue: "Password has been used recently"

**Solution**: Choose a password that hasn't been used in the last 5 password changes.

**Check History**:
```sql
SELECT COUNT(*) as PasswordHistoryCount
FROM PasswordHistory
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin');
```

### Issue: "Current password is incorrect"

**Solution**: Verify you're using the correct temporary password provided by the administrator.

### Issue: "Password change is not required"

**Solution**: This user doesn't have the `RequirePasswordChange` flag set. They can login normally.

**Check Status**:
```sql
SELECT RequirePasswordChange 
FROM AspNetUsers 
WHERE UserName = 'admin';
```

---

## Related Documentation

- [API Documentation](./API-DOCUMENTATION.md) - Complete API reference
- [Database Schema](./DATABASE-SCHEMA-DOCUMENTATION.md) - Database structure
- [Security Configuration](./SECURE-CONFIGURATION-GUIDE.md) - Security best practices
- [Troubleshooting Guide](./TROUBLESHOOTING-GUIDE.md) - Common issues and solutions

---

## Support

For additional help or questions:
1. Check the [Troubleshooting Guide](./TROUBLESHOOTING-GUIDE.md)
2. Review the [API Documentation](./API-DOCUMENTATION.md)
3. Contact your system administrator

---

**Last Updated**: 2024
**Version**: 1.0
