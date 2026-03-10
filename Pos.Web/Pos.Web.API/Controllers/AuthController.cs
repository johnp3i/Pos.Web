using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Repositories;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.Models;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Authentication controller for user login and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Handle OPTIONS preflight request
    /// </summary>
    [HttpOptions("login")]
    public IActionResult PreflightLogin()
    {
        _logger.LogInformation("OPTIONS preflight request received for /api/auth/login");
        return Ok();
    }

    /// <summary>
    /// Login endpoint - validates credentials and returns JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Error("Username and password are required"));
            }

            // Validate credentials against database
            var user = await _userRepository.ValidateCredentialsAsync(request.Username, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return Unauthorized(ApiResponse<object>.Error("Invalid username or password"));
            }

            // Generate JWT token
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresIn = _jwtTokenService.GetTokenExpirationSeconds();

            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = new UserInfo
                {
                    Id = user.ID,
                    Username = user.Name,
                    FullName = user.FullName,
                    Role = user.Role,
                    PositionTypeId = user.PositionTypeID
                }
            };

            _logger.LogInformation("Successful login for user: {Username} (ID: {UserId})", user.Name, user.ID);

            return Ok(ApiResponse<LoginResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during login"));
        }
    }

    /// <summary>
    /// Logout endpoint - invalidates current session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var username = User.Identity?.Name;
        _logger.LogInformation("Logout request from user: {Username}", username);
        
        // TODO: Implement token blacklist or session invalidation
        // For now, client-side token removal is sufficient
        
        return Ok(ApiResponse<string>.Ok("Logged out successfully"));
    }

    /// <summary>
    /// Refresh token endpoint - generates new access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate refresh token
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ApiResponse<object>.Error("Refresh token is required"));
            }

            // Validate and extract user ID from expired access token
            var userId = _jwtTokenService.ValidateToken(request.AccessToken);
            if (userId == null)
            {
                return Unauthorized(ApiResponse<object>.Error("Invalid access token"));
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Error("User not found"));
            }

            // TODO: Validate refresh token against stored tokens
            // For now, we'll generate new tokens

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresIn = _jwtTokenService.GetTokenExpirationSeconds();

            var response = new LoginResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn,
                User = new UserInfo
                {
                    Id = user.ID,
                    Username = user.Name,
                    FullName = user.FullName,
                    Role = user.Role,
                    PositionTypeId = user.PositionTypeID
                }
            };

            _logger.LogInformation("Token refreshed for user: {Username} (ID: {UserId})", user.Name, user.ID);

            return Ok(ApiResponse<LoginResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during token refresh"));
        }
    }
}

/// <summary>
/// Refresh token request model
/// </summary>
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
