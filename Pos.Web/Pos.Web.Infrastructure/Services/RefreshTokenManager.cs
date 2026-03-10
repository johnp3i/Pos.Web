using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service implementation for managing JWT refresh tokens.
/// Handles token lifecycle including creation, validation, revocation, and cleanup.
/// </summary>
public class RefreshTokenManager : IRefreshTokenManager
{
    private readonly WebPosMembershipDbContext _context;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly int _refreshTokenExpirationDays;

    public RefreshTokenManager(
        WebPosMembershipDbContext context,
        IAuditLoggingService auditLoggingService,
        IConfiguration configuration)
    {
        _context = context;
        _auditLoggingService = auditLoggingService;
        _refreshTokenExpirationDays = int.Parse(
            configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }

    /// <summary>
    /// Creates and stores a new refresh token for the specified user
    /// </summary>
    public async Task<RefreshToken> CreateRefreshTokenAsync(
        string userId, 
        string token, 
        string? deviceInfo, 
        string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Validates a refresh token and returns it if valid and active
    /// </summary>
    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return null;
        }

        // Check if token is expired
        if (refreshToken.IsExpired)
        {
            return null;
        }

        // Check if token is revoked
        if (refreshToken.IsRevoked)
        {
            return null;
        }

        return refreshToken;
    }

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    public async Task<bool> RevokeRefreshTokenAsync(string token, string reason)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedReason = reason;

        await _context.SaveChangesAsync();
        
        // Log token revocation
        await _auditLoggingService.LogSecurityEventAsync(
            Shared.Enums.AuditEventType.TokenRevoked,
            refreshToken.UserId,
            $"Refresh token revoked: {reason}",
            refreshToken.IpAddress,
            null);
        
        return true;
    }

    /// <summary>
    /// Revokes all active refresh tokens for a specific user
    /// </summary>
    public async Task<int> RevokeAllUserTokensAsync(string userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "All tokens revoked";
        }

        await _context.SaveChangesAsync();
        
        // Log token revocation
        if (activeTokens.Count > 0)
        {
            await _auditLoggingService.LogSecurityEventAsync(
                Shared.Enums.AuditEventType.TokenRevoked,
                userId,
                $"All refresh tokens revoked ({activeTokens.Count} tokens)",
                null,
                null);
        }
        
        return activeTokens.Count;
    }

    /// <summary>
    /// Deletes expired refresh tokens that are older than the specified days
    /// </summary>
    public async Task<int> CleanupExpiredTokensAsync(int olderThanDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoffDate)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        return expiredTokens.Count;
    }

    /// <summary>
    /// Gets all active refresh tokens for a specific user
    /// </summary>
    public async Task<List<RefreshToken>> GetUserTokensAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }
}
