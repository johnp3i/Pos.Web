using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Pos.Web.Client.Services.Authentication;

namespace Pos.Web.Client.Services.SignalR
{
    /// <summary>
    /// Implementation of SignalR service with automatic reconnection
    /// </summary>
    public class SignalRService : ISignalRService, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        public event EventHandler<HubConnectionState>? ConnectionStateChanged;

        public HubConnectionState ConnectionState => _hubConnection.State;

        public SignalRService(
            IConfiguration configuration,
            CustomAuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;

            // Get the API base URL from configuration
            var apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5001";

            // Build the hub connection with automatic reconnection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiBaseUrl}/hubs/main", options =>
                {
                    // Add authentication token to connection
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await _authStateProvider.GetTokenAsync();
                        return token;
                    };
                })
                .WithAutomaticReconnect(new[] 
                { 
                    TimeSpan.Zero,           // Retry immediately
                    TimeSpan.FromSeconds(2), // Then after 2 seconds
                    TimeSpan.FromSeconds(5), // Then after 5 seconds
                    TimeSpan.FromSeconds(10) // Then after 10 seconds
                })
                .Build();

            // Subscribe to connection state changes
            _hubConnection.Closed += OnConnectionClosed;
            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Reconnected += OnReconnected;
        }

        /// <summary>
        /// Starts the SignalR connection
        /// </summary>
        public async Task StartAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    RaiseConnectionStateChanged(HubConnectionState.Connected);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR connection failed: {ex.Message}");
                    // Connection will automatically retry
                }
            }
        }

        /// <summary>
        /// Stops the SignalR connection
        /// </summary>
        public async Task StopAsync()
        {
            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync();
                RaiseConnectionStateChanged(HubConnectionState.Disconnected);
            }
        }

        /// <summary>
        /// Registers a handler for a specific hub method
        /// </summary>
        public IDisposable On<T>(string methodName, Action<T> handler)
        {
            return _hubConnection.On(methodName, handler);
        }

        /// <summary>
        /// Registers a handler for a specific hub method with multiple parameters
        /// </summary>
        public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler)
        {
            return _hubConnection.On(methodName, handler);
        }

        /// <summary>
        /// Invokes a hub method
        /// </summary>
        public async Task InvokeAsync(string methodName, params object[] args)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync(methodName, args);
            }
            else
            {
                throw new InvalidOperationException("SignalR connection is not active");
            }
        }

        /// <summary>
        /// Invokes a hub method and returns a result
        /// </summary>
        public async Task<TResult> InvokeAsync<TResult>(string methodName, params object[] args)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                return await _hubConnection.InvokeAsync<TResult>(methodName, args);
            }
            else
            {
                throw new InvalidOperationException("SignalR connection is not active");
            }
        }

        private Task OnConnectionClosed(Exception? exception)
        {
            Console.WriteLine($"SignalR connection closed: {exception?.Message}");
            RaiseConnectionStateChanged(HubConnectionState.Disconnected);
            return Task.CompletedTask;
        }

        private Task OnReconnecting(Exception? exception)
        {
            Console.WriteLine($"SignalR reconnecting: {exception?.Message}");
            RaiseConnectionStateChanged(HubConnectionState.Reconnecting);
            return Task.CompletedTask;
        }

        private Task OnReconnected(string? connectionId)
        {
            Console.WriteLine($"SignalR reconnected: {connectionId}");
            RaiseConnectionStateChanged(HubConnectionState.Connected);
            return Task.CompletedTask;
        }

        private void RaiseConnectionStateChanged(HubConnectionState state)
        {
            ConnectionStateChanged?.Invoke(this, state);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
