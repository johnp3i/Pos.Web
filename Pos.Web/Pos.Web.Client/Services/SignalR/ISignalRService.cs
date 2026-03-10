using Microsoft.AspNetCore.SignalR.Client;

namespace Pos.Web.Client.Services.SignalR
{
    /// <summary>
    /// Service for managing SignalR hub connections
    /// </summary>
    public interface ISignalRService
    {
        /// <summary>
        /// Gets the current connection state
        /// </summary>
        HubConnectionState ConnectionState { get; }

        /// <summary>
        /// Starts the SignalR connection
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the SignalR connection
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Registers a handler for a specific hub method
        /// </summary>
        IDisposable On<T>(string methodName, Action<T> handler);

        /// <summary>
        /// Registers a handler for a specific hub method with multiple parameters
        /// </summary>
        IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler);

        /// <summary>
        /// Invokes a hub method
        /// </summary>
        Task InvokeAsync(string methodName, params object[] args);

        /// <summary>
        /// Invokes a hub method and returns a result
        /// </summary>
        Task<TResult> InvokeAsync<TResult>(string methodName, params object[] args);

        /// <summary>
        /// Event raised when connection state changes
        /// </summary>
        event EventHandler<HubConnectionState>? ConnectionStateChanged;
    }
}
