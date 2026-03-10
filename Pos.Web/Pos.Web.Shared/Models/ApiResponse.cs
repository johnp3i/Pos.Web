namespace Pos.Web.Shared.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Response data (null if error)
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Error message (null if successful)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Validation errors (field-level errors)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Request timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = statusCode
        };
    }
    
    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> Error(string errorMessage, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
    
    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ApiResponse<T> ValidationError(Dictionary<string, string[]> validationErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = "Validation failed",
            ValidationErrors = validationErrors,
            StatusCode = 400
        };
    }
}
