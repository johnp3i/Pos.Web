using System.Net;
using System.Net.Http.Headers;

namespace Pos.Web.Client.Services.Authentication
{
    /// <summary>
    /// HTTP message handler that adds JWT token to requests and handles token refresh
    /// </summary>
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        public AuthorizationMessageHandler(CustomAuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Get the token from storage
            var token = await _authStateProvider.GetTokenAsync();

            // Add authorization header if token exists
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            // Handle 401 Unauthorized - token might be expired
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Try to refresh the token
                var refreshToken = await _authStateProvider.GetRefreshTokenAsync();
                
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    // TODO: Implement token refresh logic
                    // This will be implemented when the API refresh endpoint is ready
                    // For now, just log out the user
                    await _authStateProvider.MarkUserAsLoggedOut();
                }
            }

            return response;
        }
    }
}
