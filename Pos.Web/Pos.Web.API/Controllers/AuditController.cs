using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs.Audit;
using Pos.Web.Shared.Enums;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Controller for audit log query operations.
/// Provides endpoints for administrators and managers to query authentication audit logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditController : ControllerBase
{
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditLoggingService auditLoggingService,
        ILogger<AuditController> logger)
    {
        _auditLoggingService = auditLoggingService;
        _logger = logger;
    }

    /// <summary>
    /// Gets audit logs for a specific user
    /// </summary>
    /// <param name="userId">User ID to query logs for</param>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <returns>List of audit log entries</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<AuthAuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AuthAuditLogDto>>> GetUserAuditLogs(
        string userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var logs = await _auditLoggingService.GetUserAuditLogsAsync(userId, from, to);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
    }

    /// <summary>
    /// Gets audit logs filtered by event type
    /// </summary>
    /// <param name="eventType">Event type to filter by</param>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <param name="limit">Maximum number of records (default: 100, max: 1000)</param>
    /// <returns>List of audit log entries</returns>
    [HttpGet("events/{eventType}")]
    [ProducesResponseType(typeof(List<AuthAuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AuthAuditLogDto>>> GetAuditLogsByEventType(
        AuditEventType eventType,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            // Validate limit
            if (limit < 1 || limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            var logs = await _auditLoggingService.GetAuditLogsByEventTypeAsync(
                eventType, 
                from, 
                to, 
                limit);
            
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs by event type: {EventType}", eventType);
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
    }

    /// <summary>
    /// Gets failed login attempts
    /// </summary>
    /// <param name="from">Start date (optional)</param>
    /// <param name="to">End date (optional)</param>
    /// <param name="limit">Maximum number of records (default: 100, max: 1000)</param>
    /// <returns>List of failed login audit log entries</returns>
    [HttpGet("failed-logins")]
    [ProducesResponseType(typeof(List<AuthAuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AuthAuditLogDto>>> GetFailedLoginAttempts(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            // Validate limit
            if (limit < 1 || limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            var logs = await _auditLoggingService.GetFailedLoginAttemptsAsync(from, to, limit);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed login attempts");
            return StatusCode(500, "An error occurred while retrieving failed login attempts");
        }
    }

    /// <summary>
    /// Queries audit logs with advanced filters
    /// </summary>
    /// <param name="request">Query request with filters</param>
    /// <returns>Paginated audit log response</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(AuditLogQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditLogQueryResponse>> QueryAuditLogs(
        [FromBody] AuditLogQueryRequest request)
    {
        try
        {
            // Validate request
            if (request.Limit < 1 || request.Limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            if (request.Page < 1)
            {
                return BadRequest("Page must be greater than 0");
            }

            List<AuthAuditLogDto> logs;

            // Query based on filters
            if (!string.IsNullOrEmpty(request.UserId))
            {
                logs = await _auditLoggingService.GetUserAuditLogsAsync(
                    request.UserId,
                    request.FromDate,
                    request.ToDate);
            }
            else if (request.EventType.HasValue)
            {
                logs = await _auditLoggingService.GetAuditLogsByEventTypeAsync(
                    request.EventType.Value,
                    request.FromDate,
                    request.ToDate,
                    request.Limit * request.Page); // Get enough for pagination
            }
            else
            {
                // If no specific filter, get failed logins as default
                logs = await _auditLoggingService.GetFailedLoginAttemptsAsync(
                    request.FromDate,
                    request.ToDate,
                    request.Limit * request.Page);
            }

            // Apply pagination
            var totalCount = logs.Count;
            var pagedLogs = logs
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToList();

            var response = new AuditLogQueryResponse
            {
                Logs = pagedLogs,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.Limit,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying audit logs");
            return StatusCode(500, "An error occurred while querying audit logs");
        }
    }
}
