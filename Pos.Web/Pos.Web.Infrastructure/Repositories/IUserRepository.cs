using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Repository interface for User operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by username (Name field)
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Get all active users
    /// </summary>
    Task<List<User>> GetActiveUsersAsync();

    /// <summary>
    /// Validate user credentials
    /// </summary>
    Task<User?> ValidateCredentialsAsync(string username, string password);
}
