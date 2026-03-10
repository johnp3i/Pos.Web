using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs.Authentication;

/// <summary>
/// Authentication result DTO returned from login and token refresh operations
/// </summary>
public class AuthenticationResultDto
{
    /// <summary>
    /// Indicates if the authentication was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// JWT access token (valid for 60 minutes)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token (valid for 7 days)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Session ID (GUID)
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code for programmatic error handling
    /// </summary>
    public AuthenticationErrorCode? ErrorCode { get; set; }

    /// <summary>
    /// Lockout end time (if account is locked)
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Creates a successful authentication result
    /// </summary>
    public static AuthenticationResultDto Success(
        string accessToken, 
        string refreshToken, 
        int expiresIn, 
        UserDto user,
        Guid sessionId)
    {
        return new AuthenticationResultDto
        {
            IsSuccessful = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            User = user,
            SessionId = sessionId
        };
    }

    /// <summary>
    /// Creates a failed authentication result
    /// </summary>
    public static AuthenticationResultDto Failure(
        string errorMessage, 
        AuthenticationErrorCode errorCode,
        DateTimeOffset? lockoutEnd = null)
    {
        return new AuthenticationResultDto
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            LockoutEnd = lockoutEnd
        };
    }
}
