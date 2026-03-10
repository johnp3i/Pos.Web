namespace Pos.Web.Shared.Enums;

/// <summary>
/// Error codes for API responses
/// </summary>
public enum ErrorCode
{
    // General errors (1000-1999)
    InternalServerError = 1000,
    ValidationError = 1001,
    InvalidOperation = 1002,
    NotFound = 1003,
    Forbidden = 1004,
    Unauthorized = 1005,
    BadRequest = 1006,
    Conflict = 1007,
    
    // Database errors (2000-2999)
    DatabaseError = 2000,
    DatabaseConnectionError = 2001,
    DatabaseTimeoutError = 2002,
    DatabaseConstraintViolation = 2003,
    
    // Authentication errors (3000-3999)
    AuthenticationFailed = 3000,
    InvalidCredentials = 3001,
    AccountLocked = 3002,
    AccountInactive = 3003,
    PasswordChangeRequired = 3004,
    InvalidToken = 3005,
    TokenExpired = 3006,
    InvalidRefreshToken = 3007,
    
    // Authorization errors (4000-4999)
    InsufficientPermissions = 4000,
    RoleNotFound = 4001,
    
    // Session errors (5000-5999)
    SessionNotFound = 5000,
    SessionExpired = 5001,
    InvalidSession = 5002,
    
    // User management errors (6000-6999)
    UserNotFound = 6000,
    UserAlreadyExists = 6001,
    InvalidPassword = 6002,
    PasswordHistoryViolation = 6003,
    
    // Migration errors (7000-7999)
    MigrationFailed = 7000,
    UserAlreadyMigrated = 7001,
    LegacyUserNotFound = 7002,
    
    // Request errors (8000-8999)
    TimeoutError = 8000,
    RequestCanceled = 8001,
    RateLimitExceeded = 8002,
    
    // External service errors (9000-9999)
    ExternalServiceError = 9000,
    ExternalServiceUnavailable = 9001,
    ExternalServiceTimeout = 9002
}
