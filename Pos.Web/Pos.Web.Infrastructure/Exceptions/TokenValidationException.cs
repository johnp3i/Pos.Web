namespace Pos.Web.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when token validation fails
/// </summary>
public class TokenValidationException : Exception
{
    public string? TokenType { get; set; }

    public TokenValidationException()
    {
    }

    public TokenValidationException(string message) : base(message)
    {
    }

    public TokenValidationException(string message, string tokenType) : base(message)
    {
        TokenType = tokenType;
    }

    public TokenValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TokenValidationException(string message, string tokenType, Exception innerException) : base(message, innerException)
    {
        TokenType = tokenType;
    }
}
