using Pos.Web.Infrastructure.Services;

namespace Pos.Web.API.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up inactive user sessions.
/// Runs every hour to end sessions that have been inactive for more than 24 hours.
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public SessionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SessionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Service started");

        // Wait 5 minutes before first cleanup to allow application to fully start
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupInactiveSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }

            // Wait for next cleanup interval
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Session Cleanup Service stopped");
    }

    private async Task CleanupInactiveSessionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting session cleanup");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();

            // Cleanup inactive sessions
            var cleanedCount = await sessionManager.CleanupInactiveSessionsAsync();

            if (cleanedCount > 0)
            {
                _logger.LogInformation(
                    "Session cleanup completed: {Count} inactive sessions ended",
                    cleanedCount);
            }
            else
            {
                _logger.LogDebug("Session cleanup completed: No inactive sessions found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up inactive sessions");
        }
    }
}
