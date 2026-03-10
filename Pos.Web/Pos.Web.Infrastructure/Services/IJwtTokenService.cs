using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT access token for legacy user (backward compatibility)
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate JWT access token for ApplicationUser with roles
    /// </summary>
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);

    /// <summary>
    /// Generate cryptographically secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token and extract user ID (legacy - returns int)
    /// </summary>
    int? ValidateToken(string token);

    /// <summary>
    /// Validate JWT token and extract user ID (returns string for ApplicationUser.Id)
    /// </summary>
    string? ValidateAccessToken(string token);

    /// <summary>
    /// Get token expiration time in seconds
    /// </summary>
    int GetTokenExpirationSeconds();
}
