namespace Pos.Web.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when password validation fails
/// </summary>
public class PasswordValidationException : Exception
{
    public List<string> ValidationErrors { get; set; } = new();

    public PasswordValidationException()
    {
    }

    public PasswordValidationException(string message) : base(message)
    {
    }

    public PasswordValidationException(string message, List<string> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public PasswordValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public PasswordValidationException(string message, List<string> validationErrors, Exception innerException) 
        : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}
