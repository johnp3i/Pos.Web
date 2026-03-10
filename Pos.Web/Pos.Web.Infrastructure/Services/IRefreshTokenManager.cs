using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for managing JWT refresh tokens.
/// Handles token creation, validation, revocation, and cleanup.
/// </summary>
public interface IRefreshTokenManager
{
    /// <summary>
    /// Creates and stores a new refresh token for the specified user
    /// </summary>
    /// <param name="userId">User ID (ApplicationUser.Id)</param>
    /// <param name="token">The refresh token string</param>
    /// <param name="deviceInfo">Device information (e.g., "Chrome on Windows")</param>
    /// <param name="ipAddress">IP address from which the token was issued</param>
    /// <returns>The created RefreshToken entity</returns>
    Task<RefreshToken> CreateRefreshTokenAsync(string userId, string token, string? deviceInfo, string? ipAddress);

    /// <summary>
    /// Validates a refresh token and returns it if valid and active
    /// </summary>
    /// <param name="token">The refresh token string to validate</param>
    /// <returns>The RefreshToken entity if valid, null otherwise</returns>
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="token">The refresh token string to revoke</param>
    /// <param name="reason">Reason for revocation (e.g., "User logout", "Password changed")</param>
    /// <returns>True if token was revoked, false if not found</returns>
    Task<bool> RevokeRefreshTokenAsync(string token, string reason);

    /// <summary>
    /// Revokes all active refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User ID (ApplicationUser.Id)</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserTokensAsync(string userId);

    /// <summary>
    /// Deletes expired refresh tokens that are older than the specified days
    /// </summary>
    /// <param name="olderThanDays">Delete tokens expired more than this many days ago (default: 30)</param>
    /// <returns>Number of tokens deleted</returns>
    Task<int> CleanupExpiredTokensAsync(int olderThanDays = 30);

    /// <summary>
    /// Gets all active refresh tokens for a specific user
    /// </summary>
    /// <param name="userId">User ID (ApplicationUser.Id)</param>
    /// <returns>List of active refresh tokens</returns>
    Task<List<RefreshToken>> GetUserTokensAsync(string userId);
}
