namespace Pos.Web.Shared.Enums;

/// <summary>
/// Authentication error codes for programmatic error handling
/// </summary>
public enum AuthenticationErrorCode
{
    /// <summary>
    /// Invalid username or password
    /// </summary>
    InvalidCredentials = 1,

    /// <summary>
    /// Account is locked due to too many failed login attempts
    /// </summary>
    AccountLocked = 2,

    /// <summary>
    /// Account is inactive/disabled
    /// </summary>
    AccountInactive = 3,

    /// <summary>
    /// User must change password before continuing
    /// </summary>
    PasswordChangeRequired = 4,

    /// <summary>
    /// Refresh token is invalid or expired
    /// </summary>
    InvalidRefreshToken = 5,

    /// <summary>
    /// Refresh token has been revoked
    /// </summary>
    RefreshTokenRevoked = 6,

    /// <summary>
    /// Two-factor authentication code required
    /// </summary>
    TwoFactorRequired = 7,

    /// <summary>
    /// Invalid two-factor authentication code
    /// </summary>
    InvalidTwoFactorCode = 8,

    /// <summary>
    /// An unexpected error occurred
    /// </summary>
    UnexpectedError = 99
}
