# Requirements Document: Web POS Membership Database

## Introduction

The Web POS Membership Database is a secure authentication and authorization system for the modern web-based MyChair POS application. This system provides ASP.NET Core Identity integration with proper password hashing, role-based access control, JWT refresh token management, and comprehensive audit logging while maintaining backward compatibility with the legacy WPF POS system.

The system enables secure user authentication, session management, token-based authorization, and comprehensive security auditing for the web-based point-of-sale application.

## Glossary

- **Authentication_System**: The Web POS Membership Database authentication and authorization system
- **Identity_Service**: ASP.NET Core Identity service managing user accounts and authentication
- **Token_Manager**: Service responsible for generating, validating, and revoking JWT tokens
- **Session_Manager**: Service tracking active user sessions and device information
- **Audit_Service**: Service logging all authentication and security events
- **Migration_Service**: Service migrating users from legacy dbo.Users table to WebPosMembership database
- **Access_Token**: Short-lived JWT token (60 minutes) used for API authorization
- **Refresh_Token**: Long-lived token (7 days) used to obtain new access tokens
- **Legacy_User**: User record in the existing dbo.Users table from WPF POS system
- **Identity_User**: User record in AspNetUsers table in WebPosMembership database
- **User_Session**: Active login session tracking device, IP address, and activity
- **Audit_Log**: Security event record in AuthAuditLog table
- **Password_Hash**: PBKDF2-hashed password stored in AspNetUsers table
- **Role**: User permission level (Admin, Manager, Cashier, Waiter, Kitchen)
- **Account_Lockout**: Temporary account suspension after failed login attempts
- **Token_Rotation**: Security practice of issuing new refresh token and revoking old one

## Requirements

### Requirement 1: User Authentication

**User Story:** As a POS user, I want to login with my username and password, so that I can access the POS system securely.

#### Acceptance Criteria

1. WHEN a user provides valid username and password, THE Authentication_System SHALL verify credentials against the Identity_Service
2. WHEN credentials are valid and account is active, THE Authentication_System SHALL generate an Access_Token and Refresh_Token
3. WHEN credentials are valid, THE Authentication_System SHALL create a User_Session record with device information and IP address
4. WHEN credentials are valid, THE Authentication_System SHALL update the user's LastLoginAt timestamp
5. WHEN credentials are valid, THE Authentication_System SHALL log a successful login event to the Audit_Service
6. WHEN credentials are invalid, THE Authentication_System SHALL increment the AccessFailedCount for the user
7. WHEN credentials are invalid, THE Authentication_System SHALL log a failed login attempt to the Audit_Service
8. WHEN credentials are invalid, THE Authentication_System SHALL return a generic error message without revealing whether username or password was incorrect
9. WHEN a user's AccessFailedCount reaches 5 attempts, THE Authentication_System SHALL lock the account for 15 minutes
10. WHEN an account is locked, THE Authentication_System SHALL prevent login even with correct credentials until lockout expires

### Requirement 2: Token Management

**User Story:** As a POS user, I want my session to remain active without frequent re-login, so that I can work efficiently without interruption.

#### Acceptance Criteria

1. WHEN generating an Access_Token, THE Token_Manager SHALL create a JWT token with 60-minute expiration
2. WHEN generating an Access_Token, THE Token_Manager SHALL include UserId, UserName, Role, and EmployeeId claims
3. WHEN generating a Refresh_Token, THE Token_Manager SHALL create a cryptographically secure 32-byte random token
4. WHEN generating a Refresh_Token, THE Token_Manager SHALL store the token in the RefreshTokens table with 7-day expiration
5. WHEN storing a Refresh_Token, THE Token_Manager SHALL record device information and IP address
6. WHEN a user requests token refresh with a valid Refresh_Token, THE Token_Manager SHALL generate new Access_Token and Refresh_Token
7. WHEN refreshing tokens, THE Token_Manager SHALL revoke the old Refresh_Token immediately
8. WHEN a Refresh_Token is expired, THE Token_Manager SHALL reject the refresh request and revoke the token
9. WHEN a Refresh_Token is revoked, THE Token_Manager SHALL reject any refresh attempts using that token
10. WHEN validating an Access_Token, THE Token_Manager SHALL verify signature, expiration, issuer, and audience

### Requirement 3: Session Management

**User Story:** As a system administrator, I want to track active user sessions, so that I can monitor system usage and manage security.

#### Acceptance Criteria

1. WHEN a user logs in successfully, THE Session_Manager SHALL create a User_Session record with unique SessionId
2. WHEN creating a User_Session, THE Session_Manager SHALL record DeviceType, DeviceInfo, IpAddress, and UserAgent
3. WHEN a user makes an API request, THE Session_Manager SHALL update the LastActivityAt timestamp for their session
4. WHEN a user logs out, THE Session_Manager SHALL set the EndedAt timestamp for their session
5. WHEN querying active sessions, THE Session_Manager SHALL return only sessions where EndedAt is null
6. WHEN a session is inactive for 24 hours, THE Session_Manager SHALL automatically end the session
7. WHEN an administrator revokes all user sessions, THE Session_Manager SHALL set EndedAt for all active sessions for that user
8. WHEN a user has multiple active sessions, THE Session_Manager SHALL allow concurrent sessions from different devices

### Requirement 4: Audit Logging

**User Story:** As a security officer, I want comprehensive logs of all authentication events, so that I can investigate security incidents and ensure compliance.

#### Acceptance Criteria

1. WHEN a login attempt occurs, THE Audit_Service SHALL log the event with username, timestamp, IP address, and success status
2. WHEN a login fails, THE Audit_Service SHALL log the event even if the username does not exist
3. WHEN a password is changed, THE Audit_Service SHALL log the event with UserId and timestamp
4. WHEN an account is locked, THE Audit_Service SHALL log the event with reason and timestamp
5. WHEN a token is refreshed, THE Audit_Service SHALL log the event with UserId and success status
6. WHEN a security event occurs, THE Audit_Service SHALL log the event type, details, and timestamp
7. WHEN logging an event, THE Audit_Service SHALL store IP address and user agent information
8. WHEN querying audit logs, THE Audit_Service SHALL support filtering by UserId, EventType, and date range
9. WHEN audit logs are older than 1 year, THE Audit_Service SHALL archive them to cold storage
10. THE Audit_Service SHALL ensure audit logs are append-only and protected from tampering

### Requirement 5: User Migration

**User Story:** As a system administrator, I want to migrate existing users from the legacy system, so that they can access the new web POS without manual account creation.

#### Acceptance Criteria

1. WHEN migration is initiated, THE Migration_Service SHALL read all active users from the legacy dbo.Users table
2. WHEN migrating a user, THE Migration_Service SHALL create an Identity_User record with EmployeeId linking to the Legacy_User
3. WHEN migrating a user, THE Migration_Service SHALL generate a secure temporary password of at least 12 characters
4. WHEN migrating a user, THE Migration_Service SHALL hash the temporary password using PBKDF2 before storing
5. WHEN migrating a user, THE Migration_Service SHALL map the PositionTypeID to the appropriate Role
6. WHEN migrating a user, THE Migration_Service SHALL set RequirePasswordChange flag to true
7. WHEN a user already exists in the Identity_Service, THE Migration_Service SHALL skip that user
8. WHEN migration fails for a specific user, THE Migration_Service SHALL continue processing remaining users
9. WHEN migration completes, THE Migration_Service SHALL return a report with success count, failure count, and error details
10. WHEN migration fails for a user, THE Migration_Service SHALL log the error with Legacy_User ID and error message

### Requirement 6: Password Security

**User Story:** As a security officer, I want strong password policies enforced, so that user accounts are protected from unauthorized access.

#### Acceptance Criteria

1. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least 8 characters
2. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least one digit
3. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least one lowercase letter
4. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least one uppercase letter
5. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least one non-alphanumeric character
6. WHEN a user creates or changes a password, THE Identity_Service SHALL require at least 4 unique characters
7. WHEN storing a password, THE Identity_Service SHALL hash it using PBKDF2 with 100,000 iterations
8. WHEN a user changes their password, THE Identity_Service SHALL store the old Password_Hash in PasswordHistory table
9. WHEN a user attempts to reuse a password, THE Identity_Service SHALL check the last 5 passwords and reject if found
10. WHEN a migrated user logs in for the first time, THE Identity_Service SHALL require password change before granting access

### Requirement 7: Role-Based Authorization

**User Story:** As a system administrator, I want to assign roles to users, so that I can control access to different POS features.

#### Acceptance Criteria

1. THE Identity_Service SHALL support five predefined roles: Admin, Manager, Cashier, Waiter, and Kitchen
2. WHEN a user is migrated, THE Migration_Service SHALL assign a role based on their PositionTypeID
3. WHEN PositionTypeID is 1, THE Migration_Service SHALL assign the Cashier role
4. WHEN PositionTypeID is 2, THE Migration_Service SHALL assign the Admin role
5. WHEN PositionTypeID is 3, THE Migration_Service SHALL assign the Manager role
6. WHEN PositionTypeID is 4, THE Migration_Service SHALL assign the Waiter role
7. WHEN PositionTypeID is 5, THE Migration_Service SHALL assign the Kitchen role
8. WHEN generating an Access_Token, THE Token_Manager SHALL include the user's role as a claim
9. WHEN a user has multiple roles, THE Token_Manager SHALL include all roles in the Access_Token claims
10. THE Identity_Service SHALL prevent deletion of system roles (Admin, Manager, Cashier, Waiter, Kitchen)

### Requirement 8: Account Lockout

**User Story:** As a security officer, I want accounts to lock after repeated failed login attempts, so that brute force attacks are prevented.

#### Acceptance Criteria

1. WHEN a login attempt fails, THE Identity_Service SHALL increment the AccessFailedCount for the user
2. WHEN AccessFailedCount reaches 5, THE Identity_Service SHALL set LockoutEnd to 15 minutes from current time
3. WHEN an account is locked, THE Identity_Service SHALL prevent login attempts until LockoutEnd expires
4. WHEN a login succeeds, THE Identity_Service SHALL reset AccessFailedCount to 0
5. WHEN LockoutEnd expires, THE Identity_Service SHALL automatically unlock the account
6. WHEN an account is locked, THE Authentication_System SHALL return an error message indicating lockout and time remaining
7. WHEN an administrator manually unlocks an account, THE Identity_Service SHALL set LockoutEnd to null and reset AccessFailedCount
8. WHEN an account is locked multiple times, THE Identity_Service SHALL increase lockout duration progressively
9. WHEN an account is locked, THE Audit_Service SHALL log the lockout event with reason
10. WHEN a locked account attempts login, THE Audit_Service SHALL log the attempt without incrementing AccessFailedCount

### Requirement 9: Token Revocation

**User Story:** As a POS user, I want to logout and invalidate my tokens, so that my session cannot be used after I leave.

#### Acceptance Criteria

1. WHEN a user logs out, THE Token_Manager SHALL revoke all active Refresh_Tokens for that user's session
2. WHEN revoking a Refresh_Token, THE Token_Manager SHALL set RevokedAt to current timestamp
3. WHEN revoking a Refresh_Token, THE Token_Manager SHALL record the RevokedReason
4. WHEN a revoked Refresh_Token is used, THE Token_Manager SHALL reject the request and log a security event
5. WHEN an administrator revokes all user sessions, THE Token_Manager SHALL revoke all Refresh_Tokens for that user
6. WHEN a Refresh_Token expires, THE Token_Manager SHALL mark it as revoked during cleanup
7. WHEN cleanup runs, THE Token_Manager SHALL delete Refresh_Tokens that expired more than 30 days ago
8. WHEN a user changes their password, THE Token_Manager SHALL revoke all existing Refresh_Tokens for that user
9. WHEN suspicious activity is detected, THE Token_Manager SHALL revoke all Refresh_Tokens and require re-authentication
10. WHEN a Refresh_Token is revoked, THE Session_Manager SHALL end the associated User_Session

### Requirement 10: Database Schema

**User Story:** As a database administrator, I want a well-structured database schema, so that the system is maintainable and performant.

#### Acceptance Criteria

1. THE Authentication_System SHALL use a separate WebPosMembership database for authentication data
2. THE Authentication_System SHALL implement all standard ASP.NET Core Identity tables
3. THE Authentication_System SHALL create a RefreshTokens table with Token, UserId, ExpiresAt, and RevokedAt columns
4. THE Authentication_System SHALL create a UserSessions table with SessionId, UserId, DeviceInfo, and timestamps
5. THE Authentication_System SHALL create an AuthAuditLog table with EventType, UserId, Timestamp, and Details columns
6. THE Authentication_System SHALL create a PasswordHistory table with UserId, PasswordHash, and CreatedAt columns
7. THE Authentication_System SHALL create a unique index on AspNetUsers.EmployeeId
8. THE Authentication_System SHALL create a unique index on RefreshTokens.Token
9. THE Authentication_System SHALL create indexes on frequently queried columns for performance
10. THE Authentication_System SHALL enforce foreign key constraints between AspNetUsers and custom tables

### Requirement 11: Legacy System Integration

**User Story:** As a system architect, I want seamless integration with the legacy POS database, so that user data remains consistent across systems.

#### Acceptance Criteria

1. WHEN creating an Identity_User, THE Authentication_System SHALL require a valid EmployeeId
2. THE Authentication_System SHALL enforce a foreign key relationship between AspNetUsers.EmployeeId and dbo.Users.ID
3. WHEN querying user information, THE Authentication_System SHALL join AspNetUsers with dbo.Users via EmployeeId
4. THE Authentication_System SHALL maintain read-only access to the legacy dbo.Users table
5. THE Authentication_System SHALL not modify the legacy dbo.Users table schema
6. WHEN a Legacy_User is deactivated, THE Authentication_System SHALL set IsActive to false for the corresponding Identity_User
7. WHEN synchronizing user data, THE Authentication_System SHALL update DisplayName from legacy FullName field
8. THE Authentication_System SHALL allow both legacy WPF POS and new web POS to coexist during transition
9. WHEN a user exists in both systems, THE Authentication_System SHALL use EmployeeId as the linking key
10. THE Authentication_System SHALL support gradual migration without requiring immediate cutover

### Requirement 12: Performance

**User Story:** As a POS user, I want fast authentication responses, so that I can start working quickly without delays.

#### Acceptance Criteria

1. WHEN a user logs in, THE Authentication_System SHALL respond within 200 milliseconds
2. WHEN a user refreshes their token, THE Token_Manager SHALL respond within 50 milliseconds
3. WHEN validating an Access_Token, THE Token_Manager SHALL complete validation within 10 milliseconds
4. WHEN logging an audit event, THE Audit_Service SHALL complete asynchronously within 20 milliseconds
5. THE Authentication_System SHALL support at least 100 concurrent login requests
6. THE Authentication_System SHALL use database connection pooling with minimum 5 and maximum 100 connections
7. THE Authentication_System SHALL cache user roles in memory for 5 minutes
8. THE Authentication_System SHALL use compiled queries for frequently executed authentication queries
9. WHEN querying audit logs, THE Audit_Service SHALL use indexed columns for filtering
10. THE Authentication_System SHALL achieve 99.9% uptime for authentication services

### Requirement 13: Error Handling

**User Story:** As a developer, I want comprehensive error handling, so that failures are graceful and debuggable.

#### Acceptance Criteria

1. WHEN database connection fails, THE Authentication_System SHALL return HTTP 503 Service Unavailable
2. WHEN database connection fails, THE Authentication_System SHALL retry with exponential backoff up to 3 attempts
3. WHEN an unexpected error occurs, THE Authentication_System SHALL log the full exception details
4. WHEN an error occurs, THE Authentication_System SHALL not expose sensitive information in error messages
5. WHEN migration fails for a user, THE Migration_Service SHALL continue processing remaining users
6. WHEN a Refresh_Token is not found, THE Token_Manager SHALL return a generic "Invalid token" error
7. WHEN token validation fails, THE Token_Manager SHALL log the failure reason for debugging
8. WHEN account lockout occurs, THE Authentication_System SHALL include lockout end time in the error response
9. WHEN a required service is unavailable, THE Authentication_System SHALL return appropriate HTTP status codes
10. THE Authentication_System SHALL use structured logging with correlation IDs for request tracing

### Requirement 14: Security Compliance

**User Story:** As a compliance officer, I want the system to meet security standards, so that we comply with regulations.

#### Acceptance Criteria

1. THE Authentication_System SHALL never store passwords in plain text
2. THE Authentication_System SHALL use parameterized queries to prevent SQL injection
3. THE Authentication_System SHALL validate and sanitize all user input
4. THE Authentication_System SHALL use HTTPS for all API endpoints
5. THE Authentication_System SHALL configure CORS to whitelist only authorized origins
6. THE Authentication_System SHALL implement rate limiting to prevent denial of service attacks
7. THE Authentication_System SHALL store JWT secret keys in secure configuration (not in source code)
8. THE Authentication_System SHALL use constant-time comparison for password verification to prevent timing attacks
9. THE Authentication_System SHALL retain audit logs for minimum 1 year for compliance
10. THE Authentication_System SHALL implement token rotation to prevent token replay attacks

### Requirement 15: Two-Factor Authentication (Optional)

**User Story:** As a security-conscious user, I want two-factor authentication, so that my account has an additional layer of security.

#### Acceptance Criteria

1. WHERE two-factor authentication is enabled, WHEN a user logs in with valid credentials, THE Authentication_System SHALL generate a two-factor code
2. WHERE two-factor authentication is enabled, WHEN a two-factor code is generated, THE Authentication_System SHALL send it via email or SMS
3. WHERE two-factor authentication is enabled, WHEN a user provides the two-factor code, THE Authentication_System SHALL validate it before issuing tokens
4. WHERE two-factor authentication is enabled, WHEN a two-factor code expires after 5 minutes, THE Authentication_System SHALL reject it
5. WHERE two-factor authentication is enabled, WHEN a two-factor code is used, THE Authentication_System SHALL invalidate it immediately
6. WHERE two-factor authentication is enabled, WHEN a user enables 2FA, THE Identity_Service SHALL set TwoFactorEnabled flag to true
7. WHERE two-factor authentication is enabled, WHEN a user disables 2FA, THE Identity_Service SHALL require password confirmation
8. WHERE two-factor authentication is enabled, THE Authentication_System SHALL provide backup codes for account recovery
9. WHERE two-factor authentication is enabled, THE Audit_Service SHALL log all 2FA events
10. WHERE two-factor authentication is enabled, THE Authentication_System SHALL support authenticator apps (TOTP)
