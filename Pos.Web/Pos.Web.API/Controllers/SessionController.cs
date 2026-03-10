using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs.Session;

namespace Pos.Web.API.Controllers;

/// <summary>
/// API controller for managing user sessions.
/// Provides endpoints to view active sessions and terminate sessions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<SessionController> _logger;

    public SessionController(
        ISessionManager sessionManager,
        ILogger<SessionController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active sessions for the current user
    /// </summary>
    /// <returns>List of active sessions</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(SessionListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sessions = await _sessionManager.GetActiveSessionsAsync(userId);

            var sessionDtos = sessions.Select(s => new UserSessionDto
            {
                SessionId = s.SessionId,
                DeviceType = s.DeviceType,
                DeviceInfo = s.DeviceInfo,
                IpAddress = s.IpAddress,
                CreatedAt = s.CreatedAt,
                LastActivityAt = s.LastActivityAt
            }).ToList();

            var response = new SessionListResponseDto
            {
                Sessions = sessionDtos,
                TotalCount = sessionDtos.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions");
            return StatusCode(500, "An error occurred while retrieving sessions");
        }
    }

    /// <summary>
    /// Ends a specific session for the current user
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Success status</returns>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EndSession(Guid sessionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify the session belongs to the current user
            var sessions = await _sessionManager.GetActiveSessionsAsync(userId);
            var session = sessions.FirstOrDefault(s => s.SessionId == sessionId);

            if (session == null)
            {
                return NotFound(new { message = "Session not found or already ended" });
            }

            var success = await _sessionManager.EndSessionAsync(sessionId);

            if (success)
            {
                _logger.LogInformation("Session ended by user: SessionId={SessionId}, UserId={UserId}", 
                    sessionId, userId);
                return Ok(new { message = "Session ended successfully" });
            }

            return StatusCode(500, new { message = "Failed to end session" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while ending the session");
        }
    }

    /// <summary>
    /// Ends all active sessions for a specific user (Admin only)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Number of sessions ended</returns>
    [HttpDelete("user/{userId}/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EndAllUserSessions(string userId)
    {
        try
        {
            var count = await _sessionManager.RevokeAllUserSessionsAsync(userId);

            _logger.LogInformation("All sessions ended for user by admin: UserId={UserId}, Count={Count}", 
                userId, count);

            return Ok(new 
            { 
                message = $"Successfully ended {count} session(s)",
                sessionsEnded = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending all sessions for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while ending sessions");
        }
    }

    /// <summary>
    /// Ends all active sessions for the current user
    /// </summary>
    /// <returns>Number of sessions ended</returns>
    [HttpDelete("all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EndAllMySessions()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var count = await _sessionManager.RevokeAllUserSessionsAsync(userId);

            _logger.LogInformation("User ended all their sessions: UserId={UserId}, Count={Count}", 
                userId, count);

            return Ok(new 
            { 
                message = $"Successfully ended {count} session(s)",
                sessionsEnded = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending all sessions for current user");
            return StatusCode(500, "An error occurred while ending sessions");
        }
    }
}
