namespace Pos.Web.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when attempting to access a locked account
/// </summary>
public class AccountLockedException : Exception
{
    public string? UserId { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    public AccountLockedException()
    {
    }

    public AccountLockedException(string message) : base(message)
    {
    }

    public AccountLockedException(string message, string userId, DateTimeOffset? lockoutEnd) : base(message)
    {
        UserId = userId;
        LockoutEnd = lockoutEnd;
    }

    public AccountLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AccountLockedException(string message, string userId, DateTimeOffset? lockoutEnd, Exception innerException) 
        : base(message, innerException)
    {
        UserId = userId;
        LockoutEnd = lockoutEnd;
    }
}
