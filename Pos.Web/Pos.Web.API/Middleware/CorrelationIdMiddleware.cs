using Serilog.Context;

namespace Pos.Web.API.Middleware;

/// <summary>
/// Middleware that generates or extracts correlation IDs for request tracking.
/// Adds correlation ID to HttpContext, response headers, and log context.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header, or generate a new one
        var correlationId = GetOrGenerateCorrelationId(context);

        // Store correlation ID in HttpContext.Items for access by other middleware/controllers
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Push correlation ID to Serilog LogContext for structured logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Also add user information if authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("sub")?.Value 
                    ?? context.User.FindFirst("userId")?.Value 
                    ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                var userName = context.User.FindFirst("userName")?.Value 
                    ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    using (LogContext.PushProperty("UserId", userId))
                    using (LogContext.PushProperty("UserName", userName ?? "Unknown"))
                    {
                        await _next(context);
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

    private string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if correlation ID is provided in request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate a new correlation ID
        return Guid.NewGuid().ToString();
    }
}

/// <summary>
/// Extension method to register the correlation ID middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
