namespace Pos.Web.Client.Services.Authentication
{
    /// <summary>
    /// Service for handling user authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        Task<AuthenticationResult> LoginAsync(string username, string password);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Refreshes the authentication token
        /// </summary>
        Task<AuthenticationResult> RefreshTokenAsync();

        /// <summary>
        /// Checks if the user is authenticated
        /// </summary>
        Task<bool> IsAuthenticatedAsync();
    }

    /// <summary>
    /// Result of an authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Username { get; set; }
        public string[]? Roles { get; set; }
    }
}
