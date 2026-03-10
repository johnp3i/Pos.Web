namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Interface for managing user sessions including creation, updates, and cleanup.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Creates a new user session with device and location information
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="deviceType">Type of device (Desktop, Tablet, Mobile)</param>
    /// <param name="deviceInfo">Detailed device information</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="userAgent">User agent string from the browser</param>
    /// <returns>Unique session identifier (GUID)</returns>
    Task<Guid> CreateSessionAsync(
        string userId,
        string deviceType,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Updates the last activity timestamp for the user's most recent active session
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="ipAddress">Current IP address</param>
    Task UpdateSessionActivityAsync(string userId, string? ipAddress);

    /// <summary>
    /// Ends a specific session by setting the EndedAt timestamp
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    Task<bool> EndSessionAsync(Guid sessionId);

    /// <summary>
    /// Gets all active sessions for a user (where EndedAt is null)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of active sessions</returns>
    Task<List<Entities.UserSession>> GetActiveSessionsAsync(string userId);

    /// <summary>
    /// Ends all active sessions for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Number of sessions ended</returns>
    Task<int> RevokeAllUserSessionsAsync(string userId);

    /// <summary>
    /// Ends sessions that have been inactive for more than 24 hours
    /// </summary>
    /// <returns>Number of sessions ended</returns>
    Task<int> CleanupInactiveSessionsAsync();
}
