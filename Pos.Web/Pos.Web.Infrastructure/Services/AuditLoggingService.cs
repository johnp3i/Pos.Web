using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Shared.DTOs.Audit;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service implementation for audit logging operations.
/// Records authentication and authorization events for security monitoring and compliance.
/// </summary>
public class AuditLoggingService : IAuditLoggingService
{
    private readonly WebPosMembershipDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuditLoggingService> _logger;

    public AuditLoggingService(
        WebPosMembershipDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AuditLoggingService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Logs a login attempt (success or failure)
    /// </summary>
    public async Task LogLoginAttemptAsync(
        string username,
        bool success,
        string? ipAddress,
        string? userAgent,
        string? errorMessage = null)
    {
        try
        {
            // Try to find user by username to get UserId
            var user = await _userManager.FindByNameAsync(username);

            var auditLog = new AuthAuditLog
            {
                UserId = user?.Id,
                UserName = username,
                EventType = success ? AuditEventType.LoginSuccess.ToString() : AuditEventType.LoginFailed.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = success,
                ErrorMessage = errorMessage,
                Details = success 
                    ? "User logged in successfully" 
                    : $"Login failed: {errorMessage ?? "Invalid credentials"}"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {EventType} for user {Username}, Success: {Success}",
                auditLog.EventType, username, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for login attempt: {Username}", username);
            // Don't throw - audit logging should not break the main flow
        }
    }

    /// <summary>
    /// Logs a logout event
    /// </summary>
    public async Task LogLogoutAsync(
        string userId,
        Guid sessionId,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = AuditEventType.Logout.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true,
                Details = $"User logged out, SessionId: {sessionId}"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: Logout for user {UserId}, SessionId: {SessionId}",
                userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for logout: {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs a password change event
    /// </summary>
    public async Task LogPasswordChangeAsync(
        string userId,
        bool success,
        string? changedBy = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            var eventType = changedBy != null && changedBy != userId
                ? AuditEventType.PasswordReset
                : AuditEventType.PasswordChanged;

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = eventType.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = success,
                Details = changedBy != null && changedBy != userId
                    ? $"Password reset by administrator: {changedBy}"
                    : "Password changed by user"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {EventType} for user {UserId}, Success: {Success}",
                eventType, userId, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for password change: {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs an account lockout event
    /// </summary>
    public async Task LogAccountLockoutAsync(
        string userId,
        string reason,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = AuditEventType.AccountLocked.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true,
                Details = $"Account locked: {reason}"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: AccountLocked for user {UserId}, Reason: {Reason}",
                userId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for account lockout: {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs an account unlock event
    /// </summary>
    public async Task LogAccountUnlockAsync(
        string userId,
        string unlockedBy,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = AuditEventType.AccountUnlocked.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true,
                Details = $"Account unlocked by: {unlockedBy}"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: AccountUnlocked for user {UserId} by {UnlockedBy}",
                userId, unlockedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for account unlock: {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs a token refresh event
    /// </summary>
    public async Task LogTokenRefreshAsync(
        string userId,
        bool success,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            var eventType = success 
                ? AuditEventType.TokenRefreshed 
                : AuditEventType.TokenRefreshFailed;

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = eventType.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = success,
                ErrorMessage = errorMessage,
                Details = success 
                    ? "Token refreshed successfully" 
                    : $"Token refresh failed: {errorMessage}"
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {EventType} for user {UserId}, Success: {Success}",
                eventType, userId, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for token refresh: {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs a generic security event
    /// </summary>
    public async Task LogSecurityEventAsync(
        AuditEventType eventType,
        string? userId,
        string details,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            ApplicationUser? user = null;
            if (!string.IsNullOrEmpty(userId))
            {
                user = await _userManager.FindByIdAsync(userId);
            }

            var auditLog = new AuthAuditLog
            {
                UserId = userId,
                UserName = user?.UserName,
                EventType = eventType.ToString(),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true,
                Details = details
            };

            _context.AuthAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {EventType} for user {UserId}, Details: {Details}",
                eventType, userId ?? "N/A", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for security event: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific user
    /// </summary>
    public async Task<List<AuthAuditLogDto>> GetUserAuditLogsAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null)
    {
        try
        {
            var query = _context.AuthAuditLogs
                .Where(log => log.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            var logs = await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(1000) // Limit to prevent excessive data retrieval
                .ToListAsync();

            return logs.Select(log => new AuthAuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                EventType = log.EventType,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Details = log.Details,
                IsSuccessful = log.IsSuccessful,
                ErrorMessage = log.ErrorMessage
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user: {UserId}", userId);
            return new List<AuthAuditLogDto>();
        }
    }

    /// <summary>
    /// Retrieves audit logs filtered by event type
    /// </summary>
    public async Task<List<AuthAuditLogDto>> GetAuditLogsByEventTypeAsync(
        AuditEventType eventType,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        try
        {
            var eventTypeString = eventType.ToString();
            var query = _context.AuthAuditLogs
                .Where(log => log.EventType == eventTypeString);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            var logs = await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToListAsync();

            return logs.Select(log => new AuthAuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                EventType = log.EventType,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Details = log.Details,
                IsSuccessful = log.IsSuccessful,
                ErrorMessage = log.ErrorMessage
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs by event type: {EventType}", eventType);
            return new List<AuthAuditLogDto>();
        }
    }

    /// <summary>
    /// Retrieves failed login attempts
    /// </summary>
    public async Task<List<AuthAuditLogDto>> GetFailedLoginAttemptsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        try
        {
            var eventTypeString = AuditEventType.LoginFailed.ToString();
            var query = _context.AuthAuditLogs
                .Where(log => log.EventType == eventTypeString);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            var logs = await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToListAsync();

            return logs.Select(log => new AuthAuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.UserName,
                EventType = log.EventType,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Details = log.Details,
                IsSuccessful = log.IsSuccessful,
                ErrorMessage = log.ErrorMessage
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed login attempts");
            return new List<AuthAuditLogDto>();
        }
    }
}
