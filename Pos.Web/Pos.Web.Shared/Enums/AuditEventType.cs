namespace Pos.Web.Shared.Enums;

/// <summary>
/// Defines the types of audit events that can be logged in the authentication system.
/// These events track security-related activities for compliance and monitoring.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// User successfully logged in
    /// </summary>
    LoginSuccess,

    /// <summary>
    /// User login attempt failed (invalid credentials)
    /// </summary>
    LoginFailed,

    /// <summary>
    /// User logged out
    /// </summary>
    Logout,

    /// <summary>
    /// User password was changed
    /// </summary>
    PasswordChanged,

    /// <summary>
    /// User account was locked due to failed login attempts
    /// </summary>
    AccountLocked,

    /// <summary>
    /// User account was unlocked by administrator
    /// </summary>
    AccountUnlocked,

    /// <summary>
    /// Access token was refreshed successfully
    /// </summary>
    TokenRefreshed,

    /// <summary>
    /// Refresh token was revoked
    /// </summary>
    TokenRevoked,

    /// <summary>
    /// User session was created
    /// </summary>
    SessionCreated,

    /// <summary>
    /// User session was ended
    /// </summary>
    SessionEnded,

    /// <summary>
    /// User role was changed
    /// </summary>
    RoleChanged,

    /// <summary>
    /// New user account was created
    /// </summary>
    UserCreated,

    /// <summary>
    /// User account was deactivated
    /// </summary>
    UserDeactivated,

    /// <summary>
    /// Generic security event (suspicious activity, etc.)
    /// </summary>
    SecurityEvent,

    /// <summary>
    /// Password reset was performed
    /// </summary>
    PasswordReset,

    /// <summary>
    /// Token refresh attempt failed
    /// </summary>
    TokenRefreshFailed
}
