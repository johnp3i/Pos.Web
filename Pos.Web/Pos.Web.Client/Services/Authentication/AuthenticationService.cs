using System.Net.Http.Json;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.Client.Services.Authentication
{
    /// <summary>
    /// Implementation of authentication service for WebPosMembership system
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        private readonly TokenRefreshService _tokenRefreshService;

        public AuthenticationService(
            HttpClient httpClient,
            CustomAuthenticationStateProvider authStateProvider,
            TokenRefreshService tokenRefreshService)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _tokenRefreshService = tokenRefreshService;
        }

        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new LoginRequestDto
                {
                    Username = username,
                    Password = password,
                    DeviceType = "Desktop", // Can be made configurable
                    RememberMe = false
                };

                var response = await _httpClient.PostAsJsonAsync("api/membership/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResultDto>();

                    if (authResult?.IsSuccessful == true && authResult.AccessToken != null)
                    {
                        // Store the tokens
                        await _authStateProvider.MarkUserAsAuthenticated(
                            authResult.AccessToken, 
                            authResult.RefreshToken ?? string.Empty);

                        // Start the token refresh service
                        await _tokenRefreshService.StartAsync();

                        return new AuthenticationResult
                        {
                            Success = true,
                            Token = authResult.AccessToken,
                            RefreshToken = authResult.RefreshToken,
                            Username = authResult.User?.UserName ?? username,
                            Roles = authResult.User?.Roles?.ToArray() ?? Array.Empty<string>()
                        };
                    }
                }

                // Handle error response
                var errorResult = await response.Content.ReadFromJsonAsync<AuthenticationResultDto>();
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = errorResult?.ErrorMessage ?? "Login failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = $"Login error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // Stop the token refresh service
                _tokenRefreshService.Stop();

                // Call the API logout endpoint for server-side session cleanup
                await _httpClient.PostAsync("api/membership/auth/logout", null);
            }
            catch
            {
                // Ignore errors - we'll log out locally anyway
            }
            finally
            {
                // Clear local authentication state
                await _authStateProvider.MarkUserAsLoggedOut();
            }
        }

        /// <summary>
        /// Refreshes the authentication token
        /// </summary>
        public async Task<AuthenticationResult> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = await _authStateProvider.GetRefreshTokenAsync();

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "No refresh token available"
                    };
                }

                var refreshRequest = new RefreshTokenRequestDto
                {
                    RefreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonAsync("api/membership/auth/refresh", refreshRequest);

                if (response.IsSuccessStatusCode)
                {
                    var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResultDto>();

                    if (authResult?.IsSuccessful == true && authResult.AccessToken != null)
                    {
                        // Store the new tokens
                        await _authStateProvider.MarkUserAsAuthenticated(
                            authResult.AccessToken,
                            authResult.RefreshToken ?? string.Empty);

                        return new AuthenticationResult
                        {
                            Success = true,
                            Token = authResult.AccessToken,
                            RefreshToken = authResult.RefreshToken
                        };
                    }
                }

                // Refresh failed - log out the user
                await _authStateProvider.MarkUserAsLoggedOut();

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }
            catch (Exception ex)
            {
                await _authStateProvider.MarkUserAsLoggedOut();

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = $"Token refresh error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Checks if the user is authenticated
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
