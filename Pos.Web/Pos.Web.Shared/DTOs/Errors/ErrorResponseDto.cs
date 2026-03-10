using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs.Errors;

/// <summary>
/// Standard error response model returned by all API endpoints
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Error code identifying the type of error
    /// </summary>
    public ErrorCode ErrorCode { get; set; }

    /// <summary>
    /// User-friendly error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (only included in development environment)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Correlation ID for tracking the request across logs
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional validation errors (for validation failures)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
