using System.Security.Claims;
using Pos.Web.Infrastructure.Services;

namespace Pos.Web.API.Middleware;

/// <summary>
/// Middleware that updates user session activity on each authenticated API request.
/// Extracts UserId from JWT claims and updates LastActivityAt timestamp.
/// </summary>
public class SessionActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionActivityMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SessionActivityMiddleware(
        RequestDelegate next,
        ILogger<SessionActivityMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Extract UserId from JWT claims
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get IP address
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                    // Update session activity asynchronously in a new scope (fire and forget)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Create a new scope to get a fresh DbContext
                            using var scope = _serviceScopeFactory.CreateScope();
                            var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();
                            await sessionManager.UpdateSessionActivityAsync(userId, ipAddress);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error updating session activity for user: {UserId}", userId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the request pipeline
                _logger.LogError(ex, "Error in SessionActivityMiddleware");
            }
        }

        // Continue to next middleware
        await _next(context);
    }
}

/// <summary>
/// Extension method for registering SessionActivityMiddleware
/// </summary>
public static class SessionActivityMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionActivity(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionActivityMiddleware>();
    }
}
