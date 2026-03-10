namespace Pos.Web.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class AuthenticationException : Exception
{
    public string? ErrorCode { get; set; }

    public AuthenticationException()
    {
    }

    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AuthenticationException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
