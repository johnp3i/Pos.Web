using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.Client.Services.Authentication
{
    /// <summary>
    /// Background service that automatically refreshes the access token before it expires
    /// </summary>
    public class TokenRefreshService : IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        private readonly ILogger<TokenRefreshService> _logger;
        private Timer? _refreshTimer;
        private bool _disposed;

        // Refresh token 5 minutes before expiration
        private const int RefreshBeforeExpirationMinutes = 5;

        public TokenRefreshService(
            IHttpClientFactory httpClientFactory,
            CustomAuthenticationStateProvider authStateProvider,
            ILogger<TokenRefreshService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        /// <summary>
        /// Starts the token refresh service
        /// </summary>
        public async Task StartAsync()
        {
            _logger.LogInformation("Token refresh service starting");

            // Check if user is authenticated by checking if token exists
            var token = await _authStateProvider.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogDebug("User not authenticated, token refresh service not started");
                return;
            }

            // Schedule the first refresh check
            await ScheduleNextRefreshAsync();
        }

        /// <summary>
        /// Stops the token refresh service
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Token refresh service stopping");
            _refreshTimer?.Dispose();
            _refreshTimer = null;
        }

        /// <summary>
        /// Schedules the next token refresh based on token expiration
        /// </summary>
        private async Task ScheduleNextRefreshAsync()
        {
            try
            {
                var token = await _authStateProvider.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogDebug("No token found, stopping refresh service");
                    Stop();
                    return;
                }

                // Parse the JWT token to get expiration time
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expirationTime = jwtToken.ValidTo;

                // Calculate when to refresh (5 minutes before expiration)
                var refreshTime = expirationTime.AddMinutes(-RefreshBeforeExpirationMinutes);
                var timeUntilRefresh = refreshTime - DateTime.UtcNow;

                if (timeUntilRefresh <= TimeSpan.Zero)
                {
                    // Token is about to expire or already expired, refresh immediately
                    _logger.LogWarning("Token is about to expire or already expired, refreshing immediately");
                    await RefreshTokenAsync();
                }
                else
                {
                    // Schedule refresh
                    _logger.LogDebug("Scheduling token refresh in {Minutes} minutes", timeUntilRefresh.TotalMinutes);
                    _refreshTimer?.Dispose();
                    _refreshTimer = new Timer(
                        async _ => await RefreshTokenAsync(),
                        null,
                        (int)timeUntilRefresh.TotalMilliseconds,
                        Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling token refresh");
                Stop();
            }
        }

        /// <summary>
        /// Refreshes the access token
        /// </summary>
        private async Task RefreshTokenAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing access token");

                var refreshToken = await _authStateProvider.GetRefreshTokenAsync();

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("No refresh token available");
                    Stop();
                    return;
                }

                var refreshRequest = new RefreshTokenRequestDto
                {
                    RefreshToken = refreshToken
                };

                // Create HttpClient from factory (avoids circular dependency)
                using var httpClient = _httpClientFactory.CreateClient("TokenRefresh");
                var response = await httpClient.PostAsJsonAsync("api/membership/auth/refresh", refreshRequest);

                if (response.IsSuccessStatusCode)
                {
                    var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResultDto>();

                    if (authResult?.IsSuccessful == true && authResult.AccessToken != null)
                    {
                        _logger.LogInformation("Access token refreshed successfully");

                        // Store the new tokens
                        await _authStateProvider.MarkUserAsAuthenticated(
                            authResult.AccessToken,
                            authResult.RefreshToken ?? string.Empty);

                        // Schedule the next refresh
                        await ScheduleNextRefreshAsync();
                        return;
                    }
                }

                _logger.LogWarning("Token refresh failed");

                // Refresh failed - log out the user
                await _authStateProvider.MarkUserAsLoggedOut();
                Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                await _authStateProvider.MarkUserAsLoggedOut();
                Stop();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _disposed = true;
            }
        }
    }
}
