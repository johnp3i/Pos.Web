using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Messages;

namespace Pos.Web.Client.Services.ServerCommand;

/// <summary>
/// Service for managing server command communication via SignalR
/// Handles device-to-master station commands (print, cash drawer, etc.)
/// </summary>
public interface IServerCommandService : IAsyncDisposable
{
    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;
    
    /// <summary>
    /// Event raised when a command is received
    /// </summary>
    event EventHandler<ServerCommandMessage>? CommandReceived;
    
    /// <summary>
    /// Event raised when a command is completed
    /// </summary>
    event EventHandler<ServerCommandCompletedEventArgs>? CommandCompleted;
    
    /// <summary>
    /// Event raised when a command fails
    /// </summary>
    event EventHandler<ServerCommandMessage>? CommandFailed;
    
    /// <summary>
    /// Gets whether the service is connected to the hub
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets the current device ID (if registered)
    /// </summary>
    string? DeviceId { get; }
    
    /// <summary>
    /// Starts the SignalR connection
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stops the SignalR connection
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Registers this device as a master station that can receive commands
    /// </summary>
    /// <param name="deviceId">Unique device identifier</param>
    Task RegisterDeviceAsync(string deviceId);
    
    /// <summary>
    /// Unregisters this device
    /// </summary>
    Task UnregisterDeviceAsync();
    
    /// <summary>
    /// Sends a command to a target device
    /// </summary>
    /// <param name="command">Command to send</param>
    Task SendCommandAsync(ServerCommandMessage command);
    
    /// <summary>
    /// Sends a print command to a specific device
    /// </summary>
    /// <param name="deviceId">Target device ID</param>
    /// <param name="printData">Print data (receipt content, format, etc.)</param>
    /// <returns>Command ID for tracking</returns>
    Task<string> SendPrintCommandAsync(string deviceId, string printData);
    
    /// <summary>
    /// Sends a cash drawer open command to a specific device
    /// </summary>
    /// <param name="deviceId">Target device ID</param>
    /// <returns>Command ID for tracking</returns>
    Task<string> SendCashDrawerCommandAsync(string deviceId);
    
    /// <summary>
    /// Notifies that a command has been completed
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <param name="result">Optional result data</param>
    Task NotifyCommandCompletedAsync(string commandId, string? result = null);
    
    /// <summary>
    /// Notifies that a command has failed
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <param name="errorMessage">Error message</param>
    Task NotifyCommandFailedAsync(string commandId, string errorMessage);
    
    /// <summary>
    /// Gets the status of a specific command
    /// </summary>
    /// <param name="commandId">Command ID</param>
    Task<ServerCommandStatus?> GetCommandStatusAsync(string commandId);
    
    /// <summary>
    /// Gets all pending commands for the current device
    /// </summary>
    Task<List<ServerCommandMessage>> GetPendingCommandsAsync();
    
    /// <summary>
    /// Gets list of registered devices
    /// </summary>
    Task<List<RegisteredDevice>> GetRegisteredDevicesAsync();
}

/// <summary>
/// Event args for command completed event
/// </summary>
public class ServerCommandCompletedEventArgs : EventArgs
{
    public string CommandId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Command status information
/// </summary>
public class ServerCommandStatus
{
    public string CommandId { get; set; } = string.Empty;
    public ServerCommandType CommandType { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Registered device information
/// </summary>
public class RegisteredDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
}
