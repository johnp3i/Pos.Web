using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service implementation for managing password history to prevent password reuse.
/// Stores last 5 passwords per user and prevents reuse.
/// </summary>
public class PasswordHistoryService : IPasswordHistoryService
{
    private readonly WebPosMembershipDbContext _context;
    private readonly ILogger<PasswordHistoryService> _logger;
    private const int MaxPasswordHistoryCount = 5;

    public PasswordHistoryService(
        WebPosMembershipDbContext context,
        ILogger<PasswordHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a password has been used recently (in last 5 passwords).
    /// </summary>
    public async Task<bool> IsPasswordRecentlyUsedAsync(string userId, string newPasswordHash)
    {
        try
        {
            // Get last 5 password hashes for the user
            var recentPasswords = await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(MaxPasswordHistoryCount)
                .Select(ph => ph.PasswordHash)
                .ToListAsync();

            // Check if new password hash matches any recent password
            return recentPasswords.Contains(newPasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password history for user: {UserId}", userId);
            // On error, allow password change (fail open for better UX)
            return false;
        }
    }

    /// <summary>
    /// Stores a password hash in the password history.
    /// </summary>
    public async Task StorePasswordHistoryAsync(
        string userId, 
        string passwordHash, 
        string changedBy, 
        string? changeReason = null)
    {
        try
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = userId,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                ChangedBy = changedBy,
                ChangeReason = changeReason ?? "Password changed"
            };

            _context.PasswordHistories.Add(passwordHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password history stored for user: {UserId}, ChangedBy: {ChangedBy}", 
                userId, changedBy);

            // Cleanup old entries (keep only last 5)
            await CleanupOldPasswordHistoryAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing password history for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Cleans up old password history entries, keeping only the last 5 per user.
    /// </summary>
    public async Task CleanupOldPasswordHistoryAsync(string userId)
    {
        try
        {
            // Get all password history entries for user, ordered by date
            var allPasswords = await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .ToListAsync();

            // If more than 5, delete the oldest ones
            if (allPasswords.Count > MaxPasswordHistoryCount)
            {
                var toDelete = allPasswords.Skip(MaxPasswordHistoryCount).ToList();
                _context.PasswordHistories.RemoveRange(toDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} old password history entries for user: {UserId}", 
                    toDelete.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up password history for user: {UserId}", userId);
            // Don't throw - cleanup failure shouldn't block password change
        }
    }
}
