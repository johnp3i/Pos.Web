using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Pos.Web.Infrastructure.Exceptions;
using Pos.Web.Shared.DTOs.Errors;
using Pos.Web.Shared.Enums;

namespace Pos.Web.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions,
/// logs them with full details, and returns appropriate HTTP responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Get correlation ID from context
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        // Log the exception with full details
        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        // Determine HTTP status code and error response based on exception type
        var (statusCode, errorCode, message, details) = GetErrorResponse(exception);

        // Create error response
        var errorResponse = new ErrorResponseDto
        {
            ErrorCode = errorCode,
            Message = message,
            Details = _environment.IsDevelopment() ? details : null, // Only include details in development
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        // Set response properties
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Add correlation ID to response headers
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        // Serialize and write response
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode statusCode, ErrorCode errorCode, string message, string? details) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            // Custom authentication exceptions
            AuthenticationException authEx => (
                HttpStatusCode.Unauthorized,
                ErrorCode.AuthenticationFailed,
                authEx.Message,
                authEx.ErrorCode
            ),

            TokenValidationException tokenEx => (
                HttpStatusCode.Unauthorized,
                ErrorCode.InvalidToken,
                tokenEx.Message,
                tokenEx.TokenType
            ),

            AccountLockedException lockEx => (
                HttpStatusCode.Forbidden,
                ErrorCode.AccountLocked,
                lockEx.Message,
                lockEx.LockoutEnd?.ToString("o")
            ),

            PasswordValidationException pwdEx => (
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidPassword,
                pwdEx.Message,
                string.Join("; ", pwdEx.ValidationErrors)
            ),

            MigrationException migEx => (
                HttpStatusCode.InternalServerError,
                ErrorCode.MigrationFailed,
                migEx.Message,
                $"LegacyUserId: {migEx.LegacyUserId}, LegacyUserName: {migEx.LegacyUserName}"
            ),

            // Database connection errors
            SqlException sqlEx when IsDatabaseConnectionError(sqlEx) => (
                HttpStatusCode.ServiceUnavailable,
                ErrorCode.DatabaseConnectionError,
                "Database service is temporarily unavailable. Please try again later.",
                sqlEx.Message
            ),

            // Database timeout errors
            SqlException sqlEx when IsDatabaseTimeoutError(sqlEx) => (
                HttpStatusCode.RequestTimeout,
                ErrorCode.DatabaseTimeoutError,
                "Database operation timed out. Please try again.",
                sqlEx.Message
            ),

            // General database errors
            SqlException sqlEx => (
                HttpStatusCode.InternalServerError,
                ErrorCode.DatabaseError,
                "A database error occurred. Please contact support if the problem persists.",
                sqlEx.Message
            ),

            // Validation errors - ArgumentNullException must come before ArgumentException
            ArgumentNullException argNullEx => (
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationError,
                $"Required parameter '{argNullEx.ParamName}' is missing.",
                argNullEx.StackTrace
            ),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationError,
                argEx.Message,
                argEx.StackTrace
            ),

            // Invalid operation errors
            InvalidOperationException invOpEx => (
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidOperation,
                invOpEx.Message,
                invOpEx.StackTrace
            ),

            // Unauthorized access
            UnauthorizedAccessException unauthEx => (
                HttpStatusCode.Forbidden,
                ErrorCode.Forbidden,
                "You do not have permission to perform this operation.",
                unauthEx.Message
            ),

            // Timeout errors
            TimeoutException timeoutEx => (
                HttpStatusCode.RequestTimeout,
                ErrorCode.TimeoutError,
                "The operation timed out. Please try again.",
                timeoutEx.Message
            ),

            // Task canceled (usually from cancellation token)
            TaskCanceledException or OperationCanceledException => (
                HttpStatusCode.RequestTimeout,
                ErrorCode.RequestCanceled,
                "The request was canceled.",
                exception.Message
            ),

            // Generic errors
            _ => (
                HttpStatusCode.InternalServerError,
                ErrorCode.InternalServerError,
                "An unexpected error occurred. Please contact support if the problem persists.",
                exception.Message
            )
        };
    }

    private bool IsDatabaseConnectionError(SqlException sqlException)
    {
        // SQL Server error codes for connection issues
        // -1 = Connection timeout
        // -2 = Connection broken
        // 53 = Could not open connection
        // 233 = Connection initialization error
        // 10053 = Transport-level error
        // 10054 = Connection forcibly closed
        // 10060 = Connection timeout
        // 10061 = Connection refused
        // 40197 = Service error processing request
        // 40501 = Service is busy
        // 40613 = Database unavailable
        var connectionErrorCodes = new[] { -1, -2, 53, 233, 10053, 10054, 10060, 10061, 40197, 40501, 40613 };
        return connectionErrorCodes.Contains(sqlException.Number);
    }

    private bool IsDatabaseTimeoutError(SqlException sqlException)
    {
        // SQL Server error code for timeout
        // -2 = Timeout expired
        return sqlException.Number == -2;
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
