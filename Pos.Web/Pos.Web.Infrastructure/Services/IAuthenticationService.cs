using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for authentication operations.
/// Handles login, logout, token refresh, and session management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="deviceType">Device type (Desktop, Tablet, Mobile)</param>
    /// <param name="deviceInfo">Device information (e.g., "Chrome on Windows")</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Authentication result with tokens and user info</returns>
    Task<AuthenticationResultDto> LoginAsync(
        string username, 
        string password, 
        string deviceType,
        string? deviceInfo, 
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="deviceInfo">Device information</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Authentication result with new tokens</returns>
    Task<AuthenticationResultDto> RefreshTokenAsync(
        string refreshToken, 
        string? deviceInfo, 
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Logs out a user by revoking tokens and ending session
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>True if logout was successful</returns>
    Task<bool> LogoutAsync(string userId, Guid sessionId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Revokes all sessions for a user (force logout from all devices)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of sessions revoked</returns>
    Task<int> RevokeAllSessionsAsync(string userId);
}
