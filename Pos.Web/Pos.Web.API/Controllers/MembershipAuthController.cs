using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs.Authentication;
using System.Security.Claims;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Authentication controller for ASP.NET Core Identity membership system.
/// Handles login, logout, token refresh, and password management with audit logging.
/// </summary>
[ApiController]
[Route("api/membership/auth")]
public class MembershipAuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly ILogger<MembershipAuthController> _logger;

    public MembershipAuthController(
        IAuthenticationService authenticationService,
        IAuditLoggingService auditLoggingService,
        UserManager<ApplicationUser> userManager,
        IPasswordHistoryService passwordHistoryService,
        ILogger<MembershipAuthController> logger)
    {
        _authenticationService = authenticationService;
        _auditLoggingService = auditLoggingService;
        _userManager = userManager;
        _passwordHistoryService = passwordHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - validates credentials and returns JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResultDto>> Login([FromBody] LoginRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var deviceInfo = $"{request.DeviceType} - {userAgent}";

        var result = await _authenticationService.LoginAsync(
            request.Username,
            request.Password,
            request.DeviceType ?? "Desktop",
            deviceInfo,
            ipAddress,
            userAgent);

        if (!result.IsSuccessful)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Token refresh endpoint - generates new access and refresh tokens
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResultDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var deviceInfo = userAgent;

        var result = await _authenticationService.RefreshTokenAsync(
            request.RefreshToken,
            deviceInfo,
            ipAddress,
            userAgent);

        if (!result.IsSuccessful)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout endpoint - revokes tokens and ends session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionIdClaim = User.FindFirstValue("SessionId");
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionIdClaim))
        {
            return Unauthorized("Invalid token");
        }

        if (!Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            return BadRequest("Invalid session ID");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var success = await _authenticationService.LogoutAsync(userId, sessionId, ipAddress, userAgent);

        if (!success)
        {
            return StatusCode(500, "Logout failed");
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Change password endpoint - changes user password with audit logging
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verify current password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            
            if (!passwordValid)
            {
                // Log failed password change attempt
                await _auditLoggingService.LogPasswordChangeAsync(
                    userId,
                    false,
                    userId,
                    ipAddress,
                    userAgent);

                return BadRequest("Current password is incorrect");
            }

            // Check password history to prevent reuse of last 5 passwords
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var newPasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            
            var isPasswordRecentlyUsed = await _passwordHistoryService.IsPasswordRecentlyUsedAsync(
                userId, 
                newPasswordHash);
            
            if (isPasswordRecentlyUsed)
            {
                // Log failed password change attempt
                await _auditLoggingService.LogPasswordChangeAsync(
                    userId,
                    false,
                    userId,
                    ipAddress,
                    userAgent);

                return BadRequest("Cannot reuse any of your last 5 passwords. Please choose a different password.");
            }

            // Store current password hash in history before changing
            var currentPasswordHash = user.PasswordHash;

            // Change password
            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                
                // Log failed password change
                await _auditLoggingService.LogPasswordChangeAsync(
                    userId,
                    false,
                    userId,
                    ipAddress,
                    userAgent);

                return BadRequest(new { message = "Password change failed", errors });
            }

            // Update last password changed timestamp
            user.LastPasswordChangedAt = DateTime.UtcNow;
            user.RequirePasswordChange = false;
            await _userManager.UpdateAsync(user);

            // Store old password hash in password history
            if (!string.IsNullOrEmpty(currentPasswordHash))
            {
                await _passwordHistoryService.StorePasswordHistoryAsync(
                    userId,
                    currentPasswordHash,
                    userId,
                    "User changed password");
            }

            // TODO: Revoke all refresh tokens (implement in Phase 7)

            // Log successful password change
            await _auditLoggingService.LogPasswordChangeAsync(
                userId,
                true,
                userId,
                ipAddress,
                userAgent);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            
            // Log failed password change
            await _auditLoggingService.LogPasswordChangeAsync(
                userId,
                false,
                userId,
                ipAddress,
                userAgent);

            return StatusCode(500, "An error occurred while changing password");
        }
    }

    /// <summary>
    /// Reset password endpoint (admin only) - resets user password with audit logging
    /// </summary>
    [HttpPost("reset-password/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordRequestDto request)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check password history to prevent reuse of last 5 passwords
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var newPasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            
            var isPasswordRecentlyUsed = await _passwordHistoryService.IsPasswordRecentlyUsedAsync(
                userId, 
                newPasswordHash);
            
            if (isPasswordRecentlyUsed)
            {
                // Log failed password reset
                await _auditLoggingService.LogPasswordChangeAsync(
                    userId,
                    false,
                    adminUserId,
                    ipAddress,
                    userAgent);

                return BadRequest(new { 
                    message = "Password reset failed", 
                    errors = "Cannot reuse any of the user's last 5 passwords. Please choose a different password." 
                });
            }

            // Store current password hash before resetting
            var currentPasswordHash = user.PasswordHash;

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                
                // Log failed password reset
                await _auditLoggingService.LogPasswordChangeAsync(
                    userId,
                    false,
                    adminUserId,
                    ipAddress,
                    userAgent);

                return BadRequest(new { message = "Password reset failed", errors });
            }

            // Set require password change flag
            user.RequirePasswordChange = true;
            user.LastPasswordChangedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Store old password hash in password history
            if (!string.IsNullOrEmpty(currentPasswordHash))
            {
                await _passwordHistoryService.StorePasswordHistoryAsync(
                    userId,
                    currentPasswordHash,
                    adminUserId ?? "System",
                    "Admin password reset");
            }

            // TODO: Revoke all refresh tokens for the user (implement in Phase 7)

            // Log successful password reset
            await _auditLoggingService.LogPasswordChangeAsync(
                userId,
                true,
                adminUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "Password reset by admin {AdminUserId} for user: {UserId}",
                adminUserId,
                userId);

            return Ok(new { message = "Password reset successfully. User must change password on next login." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            
            // Log failed password reset
            await _auditLoggingService.LogPasswordChangeAsync(
                userId,
                false,
                adminUserId,
                ipAddress,
                userAgent);

            return StatusCode(500, "An error occurred while resetting password");
        }
    }

    /// <summary>
    /// Unlock account endpoint (admin only) - unlocks locked user account
    /// </summary>
    [HttpPost("unlock-account/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockAccount(string userId)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Reset lockout
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            // Log account unlock
            await _auditLoggingService.LogAccountUnlockAsync(
                userId,
                adminUserId ?? "System",
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "Account unlocked by admin {AdminUserId} for user: {UserId}",
                adminUserId,
                userId);

            return Ok(new { message = "Account unlocked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while unlocking account");
        }
    }

    /// <summary>
    /// First-login password change endpoint - allows users to change their temporary password without a token
    /// </summary>
    [HttpPost("first-login-password-change")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResultDto>> FirstLoginPasswordChange([FromBody] FirstLoginPasswordChangeRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            // Find user by username
            var user = await _userManager.FindByNameAsync(request.Username);
            
            if (user == null)
            {
                _logger.LogWarning("First-login password change attempt for non-existent user: {Username}", request.Username);
                return Unauthorized("Invalid username or password");
            }

            // Verify current password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            
            if (!passwordValid)
            {
                _logger.LogWarning("First-login password change failed - invalid current password for user: {Username}", request.Username);
                
                // Log failed password change attempt
                await _auditLoggingService.LogPasswordChangeAsync(
                    user.Id,
                    false,
                    user.Id,
                    ipAddress,
                    userAgent);

                return Unauthorized("Invalid username or password");
            }

            // Check if user actually requires password change
            if (!user.RequirePasswordChange)
            {
                _logger.LogWarning("First-login password change attempted for user who doesn't require it: {Username}", request.Username);
                return BadRequest("Password change is not required for this account");
            }

            // Check password history to prevent reuse of last 5 passwords
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var newPasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            
            var isPasswordRecentlyUsed = await _passwordHistoryService.IsPasswordRecentlyUsedAsync(
                user.Id, 
                newPasswordHash);
            
            if (isPasswordRecentlyUsed)
            {
                _logger.LogWarning("First-login password change failed - password reuse for user: {Username}", request.Username);
                
                // Log failed password change
                await _auditLoggingService.LogPasswordChangeAsync(
                    user.Id,
                    false,
                    user.Id,
                    ipAddress,
                    userAgent);

                return BadRequest(new { 
                    message = "Password change failed", 
                    errors = "Cannot reuse any of your last 5 passwords. Please choose a different password." 
                });
            }

            // Store current password hash before changing
            var currentPasswordHash = user.PasswordHash;

            // Change password
            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                
                _logger.LogWarning("First-login password change failed for user: {Username}, Errors: {Errors}", 
                    request.Username, errors);
                
                // Log failed password change
                await _auditLoggingService.LogPasswordChangeAsync(
                    user.Id,
                    false,
                    user.Id,
                    ipAddress,
                    userAgent);

                return BadRequest(new { message = "Password change failed", errors });
            }

            // Update last password changed timestamp and clear RequirePasswordChange flag
            user.LastPasswordChangedAt = DateTime.UtcNow;
            user.RequirePasswordChange = false;
            await _userManager.UpdateAsync(user);

            // Store old password hash in password history
            if (!string.IsNullOrEmpty(currentPasswordHash))
            {
                await _passwordHistoryService.StorePasswordHistoryAsync(
                    user.Id,
                    currentPasswordHash,
                    user.Id,
                    "First login password change");
            }

            // Log successful password change
            await _auditLoggingService.LogPasswordChangeAsync(
                user.Id,
                true,
                user.Id,
                ipAddress,
                userAgent);

            _logger.LogInformation("First-login password changed successfully for user: {Username}", request.Username);

            // Now authenticate the user and return tokens
            var deviceInfo = $"{request.DeviceType ?? "Desktop"} - {userAgent}";
            
            var authResult = await _authenticationService.LoginAsync(
                request.Username,
                request.NewPassword,
                request.DeviceType ?? "Desktop",
                deviceInfo,
                ipAddress,
                userAgent);

            return Ok(authResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during first-login password change for user: {Username}", request.Username);
            return StatusCode(500, "An error occurred while changing password");
        }
    }

    /// <summary>
    /// Get current user info endpoint
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);

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

        return Ok(userDto);
    }
}
