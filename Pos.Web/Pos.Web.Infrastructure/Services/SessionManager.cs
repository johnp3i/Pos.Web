using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service implementation for managing user sessions.
/// Handles session creation, activity tracking, termination, and cleanup.
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly WebPosMembershipDbContext _context;
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(
        WebPosMembershipDbContext context,
        ILogger<SessionManager> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user session with unique GUID and device information
    /// </summary>
    public async Task<Guid> CreateSessionAsync(
        string userId,
        string deviceType,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            var session = new UserSession
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                DeviceType = deviceType,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Session created: SessionId={SessionId}, UserId={UserId}, DeviceType={DeviceType}",
                session.SessionId, userId, deviceType);

            return session.SessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Updates LastActivityAt timestamp for the user's most recent active session
    /// </summary>
    public async Task UpdateSessionActivityAsync(string userId, string? ipAddress)
    {
        try
        {
            // Find the most recent active session for the user
            var session = await _context.UserSessions
                .Where(s => s.UserId == userId && s.EndedAt == null)
                .OrderByDescending(s => s.LastActivityAt)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                
                // Update IP address if it changed
                if (!string.IsNullOrEmpty(ipAddress) && session.IpAddress != ipAddress)
                {
                    session.IpAddress = ipAddress;
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session activity for user: {UserId}", userId);
            // Don't throw - session activity update failures shouldn't break the request
        }
    }

    /// <summary>
    /// Ends a specific session by setting EndedAt timestamp
    /// </summary>
    public async Task<bool> EndSessionAsync(Guid sessionId)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return false;
            }

            if (session.EndedAt.HasValue)
            {
                _logger.LogInformation("Session already ended: {SessionId}", sessionId);
                return true;
            }

            session.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Session ended: {SessionId}, UserId={UserId}", 
                sessionId, session.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// Gets all active sessions for a user (EndedAt is null)
    /// </summary>
    public async Task<List<UserSession>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            return await _context.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.EndedAt == null)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user: {UserId}", userId);
            return new List<UserSession>();
        }
    }

    /// <summary>
    /// Ends all active sessions for a user
    /// </summary>
    public async Task<int> RevokeAllUserSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.EndedAt == null)
                .ToListAsync();

            if (activeSessions.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var session in activeSessions)
            {
                session.EndedAt = now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "All sessions revoked for user: {UserId}, Count={Count}",
                userId, activeSessions.Count);

            return activeSessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all sessions for user: {UserId}", userId);
            return 0;
        }
    }

    /// <summary>
    /// Ends sessions inactive for more than 24 hours
    /// </summary>
    public async Task<int> CleanupInactiveSessionsAsync()
    {
        try
        {
            var inactivityThreshold = DateTime.UtcNow.AddHours(-24);

            var inactiveSessions = await _context.UserSessions
                .Where(s => s.EndedAt == null && s.LastActivityAt < inactivityThreshold)
                .ToListAsync();

            if (inactiveSessions.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var session in inactiveSessions)
            {
                session.EndedAt = now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Inactive sessions cleaned up: Count={Count}, InactivityThreshold={Threshold}",
                inactiveSessions.Count, inactivityThreshold);

            return inactiveSessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up inactive sessions");
            return 0;
        }
    }
}
