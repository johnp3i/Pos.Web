namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for managing password history to prevent password reuse.
/// </summary>
public interface IPasswordHistoryService
{
    /// <summary>
    /// Checks if a password has been used recently (in last 5 passwords).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPasswordHash">Hash of the new password to check</param>
    /// <returns>True if password was used recently, false otherwise</returns>
    Task<bool> IsPasswordRecentlyUsedAsync(string userId, string newPasswordHash);

    /// <summary>
    /// Stores a password hash in the password history.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="passwordHash">Password hash to store</param>
    /// <param name="changedBy">User ID of who changed the password</param>
    /// <param name="changeReason">Reason for password change</param>
    Task StorePasswordHistoryAsync(string userId, string passwordHash, string changedBy, string? changeReason = null);

    /// <summary>
    /// Cleans up old password history entries, keeping only the last 5 per user.
    /// </summary>
    /// <param name="userId">User ID</param>
    Task CleanupOldPasswordHistoryAsync(string userId);
}
