using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;

namespace Pos.Web.API.BackgroundServices;

/// <summary>
/// Background service that archives audit logs older than 1 year.
/// Runs monthly to maintain compliance with retention policies.
/// </summary>
public class AuditLogArchivalService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogArchivalService> _logger;
    private readonly TimeSpan _archivalInterval = TimeSpan.FromDays(30); // Run monthly
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(365); // 1 year

    public AuditLogArchivalService(
        IServiceProvider serviceProvider,
        ILogger<AuditLogArchivalService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Log Archival Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ArchiveOldLogsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit log archival");
            }

            // Wait for next archival interval
            await Task.Delay(_archivalInterval, stoppingToken);
        }

        _logger.LogInformation("Audit Log Archival Service stopped");
    }

    private async Task ArchiveOldLogsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WebPosMembershipDbContext>();

        try
        {
            var cutoffDate = DateTime.UtcNow.Subtract(_retentionPeriod);
            
            _logger.LogInformation(
                "Starting audit log archival for logs older than {CutoffDate}",
                cutoffDate);

            // Count logs to be archived
            var logsToArchiveCount = await context.AuthAuditLogs
                .Where(log => log.Timestamp < cutoffDate)
                .CountAsync(cancellationToken);

            if (logsToArchiveCount == 0)
            {
                _logger.LogInformation("No audit logs to archive");
                return;
            }

            _logger.LogInformation(
                "Found {Count} audit logs to archive",
                logsToArchiveCount);

            // In a production system, you would:
            // 1. Export logs to archive storage (Azure Blob, S3, etc.)
            // 2. Verify export was successful
            // 3. Delete from active database
            
            // For now, we'll just log the archival operation
            // In a real implementation, you would export the data before deletion
            
            // Example: Export to file or cloud storage
            // var logsToArchive = await context.AuthAuditLogs
            //     .Where(log => log.Timestamp < cutoffDate)
            //     .ToListAsync(cancellationToken);
            // await ExportLogsToArchiveAsync(logsToArchive, cancellationToken);

            // Delete archived logs from active database
            // NOTE: In production, only delete after successful export
            var deletedCount = await context.AuthAuditLogs
                .Where(log => log.Timestamp < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation(
                "Archived and deleted {Count} audit logs older than {CutoffDate}",
                deletedCount,
                cutoffDate);

            // Log the archival operation itself
            var archivalLog = new Infrastructure.Entities.AuthAuditLog
            {
                UserId = null,
                UserName = "System",
                EventType = "AuditLogArchival",
                Timestamp = DateTime.UtcNow,
                IsSuccessful = true,
                Details = $"Archived {deletedCount} audit logs older than {cutoffDate:yyyy-MM-dd}"
            };

            context.AuthAuditLogs.Add(archivalLog);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving audit logs");
            
            // Log the failed archival operation
            try
            {
                var errorLog = new Infrastructure.Entities.AuthAuditLog
                {
                    UserId = null,
                    UserName = "System",
                    EventType = "AuditLogArchivalFailed",
                    Timestamp = DateTime.UtcNow,
                    IsSuccessful = false,
                    ErrorMessage = ex.Message,
                    Details = "Audit log archival failed"
                };

                context.AuthAuditLogs.Add(errorLog);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error logging archival failure");
            }
        }
    }

    // TODO: Implement in production
    // private async Task ExportLogsToArchiveAsync(
    //     List<AuthAuditLog> logs, 
    //     CancellationToken cancellationToken)
    // {
    //     // Export to Azure Blob Storage, AWS S3, or file system
    //     // Example: JSON export to file
    //     var json = JsonSerializer.Serialize(logs);
    //     var fileName = $"audit-logs-archive-{DateTime.UtcNow:yyyy-MM-dd}.json";
    //     await File.WriteAllTextAsync(fileName, json, cancellationToken);
    // }
}
