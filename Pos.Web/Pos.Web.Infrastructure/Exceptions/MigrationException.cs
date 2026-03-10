namespace Pos.Web.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when user migration fails
/// </summary>
public class MigrationException : Exception
{
    public int? LegacyUserId { get; set; }
    public string? LegacyUserName { get; set; }

    public MigrationException()
    {
    }

    public MigrationException(string message) : base(message)
    {
    }

    public MigrationException(string message, int legacyUserId, string legacyUserName) : base(message)
    {
        LegacyUserId = legacyUserId;
        LegacyUserName = legacyUserName;
    }

    public MigrationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public MigrationException(string message, int legacyUserId, string legacyUserName, Exception innerException) 
        : base(message, innerException)
    {
        LegacyUserId = legacyUserId;
        LegacyUserName = legacyUserName;
    }
}
