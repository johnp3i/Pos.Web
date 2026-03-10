using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Shared.DTOs.Authentication;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service implementation for authentication operations.
/// Handles login, logout, token refresh, and session management with ASP.NET Core Identity.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly ISessionManager _sessionManager;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly WebPosMembershipDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenManager refreshTokenManager,
        ISessionManager sessionManager,
        IAuditLoggingService auditLoggingService,
        WebPosMembershipDbContext context,
        IMemoryCache cache,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenManager = refreshTokenManager;
        _sessionManager = sessionManager;
        _auditLoggingService = auditLoggingService;
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    public async Task<AuthenticationResultDto> LoginAsync(
        string username,
        string password,
        string deviceType,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            // Find user by username
            var user = await _userManager.FindByNameAsync(username);
            
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", username);
                
                // Log failed login attempt even for non-existent users
                await _auditLoggingService.LogLoginAttemptAsync(
                    username, 
                    false, 
                    ipAddress, 
                    userAgent,
                    "User not found");
                
                return AuthenticationResultDto.Failure(
                    "Invalid username or password",
                    AuthenticationErrorCode.InvalidCredentials);
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive account: {Username}", username);
                
                // Log failed login attempt
                await _auditLoggingService.LogLoginAttemptAsync(
                    username, 
                    false, 
                    ipAddress, 
                    userAgent,
                    "Account is inactive");
                
                return AuthenticationResultDto.Failure(
                    "Account is inactive. Please contact administrator.",
                    AuthenticationErrorCode.AccountInactive);
            }

            // Check if account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                _logger.LogWarning("Login attempt for locked account: {Username}, LockoutEnd: {LockoutEnd}", 
                    username, lockoutEnd);
                
                // Log failed login attempt
                await _auditLoggingService.LogLoginAttemptAsync(
                    username, 
                    false, 
                    ipAddress, 
                    userAgent,
                    "Account is locked");
                
                return AuthenticationResultDto.Failure(
                    $"Account is locked until {lockoutEnd?.LocalDateTime:g}. Please try again later.",
                    AuthenticationErrorCode.AccountLocked,
                    lockoutEnd);
            }

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            
            if (!passwordValid)
            {
                // Increment failed access count
                await _userManager.AccessFailedAsync(user);
                
                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                _logger.LogWarning("Failed login attempt for user: {Username}, FailedCount: {FailedCount}", 
                    username, failedCount);

                // Check if account is now locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    _logger.LogWarning("Account locked due to failed attempts: {Username}", username);
                    
                    // Log account lockout event
                    await _auditLoggingService.LogAccountLockoutAsync(
                        user.Id,
                        "Maximum failed login attempts exceeded",
                        ipAddress,
                        userAgent);
                    
                    // Log failed login attempt
                    await _auditLoggingService.LogLoginAttemptAsync(
                        username, 
                        false, 
                        ipAddress, 
                        userAgent,
                        "Invalid password - account locked");
                    
                    return AuthenticationResultDto.Failure(
                        $"Account is locked until {lockoutEnd?.LocalDateTime:g} due to too many failed login attempts.",
                        AuthenticationErrorCode.AccountLocked,
                        lockoutEnd);
                }

                // Log failed login attempt
                await _auditLoggingService.LogLoginAttemptAsync(
                    username, 
                    false, 
                    ipAddress, 
                    userAgent,
                    "Invalid password");

                return AuthenticationResultDto.Failure(
                    "Invalid username or password",
                    AuthenticationErrorCode.InvalidCredentials);
            }

            // Check if password change is required
            if (user.RequirePasswordChange)
            {
                _logger.LogInformation("Password change required for user: {Username}", username);
                return AuthenticationResultDto.Failure(
                    "You must change your password before logging in.",
                    AuthenticationErrorCode.PasswordChangeRequired);
            }

            // Reset failed access count on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            // Get user roles (with caching)
            var roles = await GetUserRolesWithCacheAsync(user);

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresIn = _jwtTokenService.GetTokenExpirationSeconds();

            // Store refresh token
            await _refreshTokenManager.CreateRefreshTokenAsync(
                user.Id, 
                refreshToken, 
                deviceInfo, 
                ipAddress);

            // Create user session using SessionManager
            var sessionId = await _sessionManager.CreateSessionAsync(
                user.Id,
                deviceType,
                deviceInfo,
                ipAddress,
                userAgent);

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Successful login for user: {Username}, SessionId: {SessionId}", 
                username, sessionId);

            // Log successful login
            await _auditLoggingService.LogLoginAttemptAsync(
                username, 
                true, 
                ipAddress, 
                userAgent);

            // Create user DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                DisplayName = user.DisplayName,
                EmployeeId = user.EmployeeId,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                RequirePasswordChange = user.RequirePasswordChange
            };

            return AuthenticationResultDto.Success(
                accessToken,
                refreshToken,
                expiresIn,
                userDto,
                sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return AuthenticationResultDto.Failure(
                "An unexpected error occurred during login",
                AuthenticationErrorCode.UnexpectedError);
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    public async Task<AuthenticationResultDto> RefreshTokenAsync(
        string refreshToken,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            // Validate refresh token
            var storedToken = await _refreshTokenManager.ValidateRefreshTokenAsync(refreshToken);
            
            if (storedToken == null)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                
                // Log failed token refresh (we don't have userId yet)
                await _auditLoggingService.LogSecurityEventAsync(
                    Shared.Enums.AuditEventType.TokenRefreshFailed,
                    null,
                    "Invalid or expired refresh token used",
                    ipAddress,
                    userAgent);
                
                return AuthenticationResultDto.Failure(
                    "Invalid or expired refresh token",
                    AuthenticationErrorCode.InvalidRefreshToken);
            }

            // Get user
            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            
            if (user == null)
            {
                _logger.LogError("User not found for refresh token: {UserId}", storedToken.UserId);
                return AuthenticationResultDto.Failure(
                    "User not found",
                    AuthenticationErrorCode.UnexpectedError);
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Token refresh attempt for inactive account: {UserId}", user.Id);
                
                // Log failed token refresh
                await _auditLoggingService.LogTokenRefreshAsync(
                    user.Id,
                    false,
                    ipAddress,
                    userAgent,
                    "Account is inactive");
                
                return AuthenticationResultDto.Failure(
                    "Account is inactive",
                    AuthenticationErrorCode.AccountInactive);
            }

            // Check if account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                _logger.LogWarning("Token refresh attempt for locked account: {UserId}", user.Id);
                
                // Log failed token refresh
                await _auditLoggingService.LogTokenRefreshAsync(
                    user.Id,
                    false,
                    ipAddress,
                    userAgent,
                    "Account is locked");
                
                return AuthenticationResultDto.Failure(
                    "Account is locked",
                    AuthenticationErrorCode.AccountLocked,
                    lockoutEnd);
            }

            // Get user roles (with caching)
            var roles = await GetUserRolesWithCacheAsync(user);

            // Generate new tokens (token rotation)
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresIn = _jwtTokenService.GetTokenExpirationSeconds();

            // Revoke old refresh token
            await _refreshTokenManager.RevokeRefreshTokenAsync(refreshToken, "Token rotated");

            // Store new refresh token
            await _refreshTokenManager.CreateRefreshTokenAsync(
                user.Id,
                newRefreshToken,
                deviceInfo,
                ipAddress);

            // Update session activity
            var session = await _context.UserSessions
                .Where(s => s.UserId == user.Id && s.EndedAt == null)
                .OrderByDescending(s => s.LastActivityAt)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

            // Log successful token refresh
            await _auditLoggingService.LogTokenRefreshAsync(
                user.Id,
                true,
                ipAddress,
                userAgent);

            // Create user DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                DisplayName = user.DisplayName,
                EmployeeId = user.EmployeeId,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                RequirePasswordChange = user.RequirePasswordChange
            };

            return AuthenticationResultDto.Success(
                newAccessToken,
                newRefreshToken,
                expiresIn,
                userDto,
                session?.SessionId ?? Guid.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return AuthenticationResultDto.Failure(
                "An unexpected error occurred during token refresh",
                AuthenticationErrorCode.UnexpectedError);
        }
    }

    /// <summary>
    /// Logs out a user by revoking tokens and ending session
    /// </summary>
    public async Task<bool> LogoutAsync(string userId, Guid sessionId, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            // End the session first
            await _sessionManager.EndSessionAsync(sessionId);

            // Revoke all refresh tokens for the user
            await _refreshTokenManager.RevokeAllUserTokensAsync(userId);

            _logger.LogInformation("User logged out: {UserId}, SessionId: {SessionId}", userId, sessionId);
            
            // Log logout event
            await _auditLoggingService.LogLogoutAsync(
                userId,
                sessionId,
                ipAddress,
                userAgent);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Revokes all sessions for a user (force logout from all devices)
    /// </summary>
    public async Task<int> RevokeAllSessionsAsync(string userId)
    {
        try
        {
            // Revoke all refresh tokens
            await _refreshTokenManager.RevokeAllUserTokensAsync(userId);

            // End all active sessions using SessionManager
            var count = await _sessionManager.RevokeAllUserSessionsAsync(userId);

            _logger.LogInformation("All sessions revoked for user: {UserId}, Count: {Count}", 
                userId, count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all sessions for user: {UserId}", userId);
            return 0;
        }
    }

    /// <summary>
    /// Gets user roles with caching (5-minute expiration)
    /// </summary>
    private async Task<IList<string>> GetUserRolesWithCacheAsync(ApplicationUser user)
    {
        var cacheKey = $"UserRoles_{user.Id}";
        
        if (_cache.TryGetValue(cacheKey, out IList<string>? cachedRoles) && cachedRoles != null)
        {
            _logger.LogDebug("User roles retrieved from cache for user: {UserId}", user.Id);
            return cachedRoles;
        }

        // Cache miss - get roles from database
        var roles = await _userManager.GetRolesAsync(user);
        
        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        _cache.Set(cacheKey, roles, cacheOptions);
        
        _logger.LogDebug("User roles cached for user: {UserId}", user.Id);
        
        return roles;
    }

    /// <summary>
    /// Invalidates the role cache for a specific user
    /// </summary>
    public void InvalidateUserRoleCache(string userId)
    {
        var cacheKey = $"UserRoles_{userId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("User role cache invalidated for user: {UserId}", userId);
    }
}
