using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// API audit log service implementation for tracking API operations
/// Records all API requests, responses, and entity changes with automatic change tracking
/// </summary>
public class ApiAuditLogService : IApiAuditLogService
{
    private readonly PosDbContext _context;
    private readonly ILogger<ApiAuditLogService> _logger;

    public ApiAuditLogService(
        PosDbContext context,
        ILogger<ApiAuditLogService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Log an API request with response details
    /// </summary>
    public async Task LogApiRequestAsync(
        int? userId,
        string action,
        string requestPath,
        string requestMethod,
        int statusCode,
        int duration,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null)
    {
        try
        {
            var auditLog = new ApiAuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserID = userId,
                Action = action,
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                StatusCode = statusCode,
                Duration = duration,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                ErrorMessage = errorMessage
            };

            _context.ApiAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogDebug(
                "API request logged: {Action} {Method} {Path} - Status: {StatusCode}, Duration: {Duration}ms",
                action, requestMethod, requestPath, statusCode, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging API request: {Action} {Method} {Path}", action, requestMethod, requestPath);
            // Don't throw - audit logging should not break the main flow
        }
    }

    /// <summary>
    /// Log an entity change (create, update, delete)
    /// </summary>
    public async Task LogEntityChangeAsync(
        int? userId,
        string action,
        string entityType,
        int entityId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var auditLog = new ApiAuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserID = userId,
                Action = action,
                EntityType = entityType,
                EntityID = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                StatusCode = 200 // Assume success for entity changes
            };

            _context.ApiAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Entity change logged: {Action} {EntityType} ID: {EntityId} by User: {UserId}",
                action, entityType, entityId, userId ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging entity change: {Action} {EntityType} ID: {EntityId}", action, entityType, entityId);
            // Don't throw - audit logging should not break the main flow
        }
    }

    /// <summary>
    /// Log an entity change with automatic change tracking
    /// Compares old and new entity states and serializes differences
    /// </summary>
    public async Task LogEntityChangeWithTrackingAsync<T>(
        int? userId,
        string action,
        T? oldEntity,
        T? newEntity,
        string? ipAddress = null,
        string? userAgent = null) where T : class
    {
        try
        {
            var entityType = typeof(T).Name;
            int? entityId = null;
            string? oldValues = null;
            string? newValues = null;

            // Serialize old entity
            if (oldEntity != null)
            {
                oldValues = JsonSerializer.Serialize(oldEntity, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Try to get entity ID from old entity
                entityId = GetEntityId(oldEntity);
            }

            // Serialize new entity
            if (newEntity != null)
            {
                newValues = JsonSerializer.Serialize(newEntity, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Try to get entity ID from new entity if not already set
                entityId ??= GetEntityId(newEntity);
            }

            if (entityId.HasValue)
            {
                await LogEntityChangeAsync(
                    userId,
                    action,
                    entityType,
                    entityId.Value,
                    oldValues,
                    newValues,
                    ipAddress,
                    userAgent);
            }
            else
            {
                _logger.LogWarning("Could not determine entity ID for {EntityType} change tracking", entityType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging entity change with tracking: {Action} {EntityType}", action, typeof(T).Name);
            // Don't throw - audit logging should not break the main flow
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    public async Task<List<ApiAuditLog>> GetEntityAuditLogsAsync(
        string entityType,
        int entityId,
        DateTime? from = null,
        DateTime? to = null)
    {
        try
        {
            var query = _context.ApiAuditLogs
                .Where(log => log.EntityType == entityType && log.EntityID == entityId);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(1000) // Limit to prevent excessive data retrieval
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity audit logs: {EntityType} ID: {EntityId}", entityType, entityId);
            return new List<ApiAuditLog>();
        }
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    public async Task<List<ApiAuditLog>> GetUserAuditLogsAsync(
        int userId,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        try
        {
            var query = _context.ApiAuditLogs
                .Where(log => log.UserID == userId);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user audit logs: UserId: {UserId}", userId);
            return new List<ApiAuditLog>();
        }
    }

    /// <summary>
    /// Get failed API requests (4xx and 5xx status codes)
    /// </summary>
    public async Task<List<ApiAuditLog>> GetFailedRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        try
        {
            var query = _context.ApiAuditLogs
                .Where(log => log.StatusCode >= 400);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed API requests");
            return new List<ApiAuditLog>();
        }
    }

    /// <summary>
    /// Get audit logs by action
    /// </summary>
    public async Task<List<ApiAuditLog>> GetAuditLogsByActionAsync(
        string action,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        try
        {
            var query = _context.ApiAuditLogs
                .Where(log => log.Action == action);

            if (from.HasValue)
            {
                query = query.Where(log => log.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.Timestamp <= to.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs by action: {Action}", action);
            return new List<ApiAuditLog>();
        }
    }

    /// <summary>
    /// Try to get entity ID from an entity using reflection
    /// Looks for common ID property names: ID, Id, {EntityType}ID, {EntityType}Id
    /// </summary>
    private int? GetEntityId<T>(T entity) where T : class
    {
        try
        {
            var type = typeof(T);
            var entityTypeName = type.Name;

            // Try common ID property names
            var idPropertyNames = new[]
            {
                "ID",
                "Id",
                $"{entityTypeName}ID",
                $"{entityTypeName}Id"
            };

            foreach (var propertyName in idPropertyNames)
            {
                var property = type.GetProperty(propertyName);
                if (property != null && property.PropertyType == typeof(int))
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        return (int)value;
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting entity ID from {EntityType}", typeof(T).Name);
            return null;
        }
    }
}
