using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pos.Web.Client.Services.Authentication;
using Pos.Web.Shared.Constants;
using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Messages;
using System.Text.Json;

namespace Pos.Web.Client.Services.ServerCommand;

/// <summary>
/// Implementation of server command service with SignalR
/// </summary>
public class ServerCommandService : IServerCommandService
{
    private readonly IConfiguration _configuration;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ServerCommandService> _logger;
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hubConnection;
    private string? _deviceId;
    
    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<ServerCommandMessage>? CommandReceived;
    public event EventHandler<ServerCommandCompletedEventArgs>? CommandCompleted;
    public event EventHandler<ServerCommandMessage>? CommandFailed;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public string? DeviceId => _deviceId;
    
    public ServerCommandService(
        IConfiguration configuration,
        CustomAuthenticationStateProvider authStateProvider,
        ILogger<ServerCommandService> logger,
        NavigationManager navigationManager)
    {
        _configuration = configuration;
        _authStateProvider = authStateProvider;
        _logger = logger;
        _navigationManager = navigationManager;
    }
    
    /// <summary>
    /// Starts the SignalR connection
    /// </summary>
    public async Task StartAsync()
    {
        if (_hubConnection != null)
        {
            _logger.LogWarning("ServerCommandService already started");
            return;
        }
        
        try
        {
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:5001";
            
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiBaseUrl}{Hubs.ServerCommand}", options =>
                {
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
            
            // Subscribe to hub messages
            _hubConnection.On<ServerCommandMessage>(
                SignalRMethods.ServerCommand.CommandReceived,
                OnCommandReceived);
            
            _hubConnection.On<object>(
                SignalRMethods.ServerCommand.CommandCompleted,
                OnCommandCompletedMessage);
            
            _hubConnection.On<ServerCommandMessage>(
                SignalRMethods.ServerCommand.CommandFailed,
                OnCommandFailedMessage);
            
            await _hubConnection.StartAsync();
            
            ConnectionStateChanged?.Invoke(this, true);
            
            _logger.LogInformation("ServerCommandService connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ServerCommandService");
            ConnectionStateChanged?.Invoke(this, false);
            throw;
        }
    }
    
    /// <summary>
    /// Stops the SignalR connection
    /// </summary>
    public async Task StopAsync()
    {
        if (_hubConnection == null)
        {
            return;
        }
        
        try
        {
            // Unregister device if registered
            if (!string.IsNullOrEmpty(_deviceId))
            {
                await UnregisterDeviceAsync();
            }
            
            await _hubConnection.StopAsync();
            
            ConnectionStateChanged?.Invoke(this, false);
            
            _logger.LogInformation("ServerCommandService stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping ServerCommandService");
        }
    }
    
    /// <summary>
    /// Registers this device as a master station
    /// </summary>
    public async Task RegisterDeviceAsync(string deviceId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            await _hubConnection.InvokeAsync("RegisterDevice", deviceId);
            _deviceId = deviceId;
            
            _logger.LogInformation("Device registered: {DeviceId}", deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register device: {DeviceId}", deviceId);
            throw;
        }
    }
    
    /// <summary>
    /// Unregisters this device
    /// </summary>
    public async Task UnregisterDeviceAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected || string.IsNullOrEmpty(_deviceId))
        {
            return;
        }
        
        try
        {
            await _hubConnection.InvokeAsync("UnregisterDevice", _deviceId);
            
            _logger.LogInformation("Device unregistered: {DeviceId}", _deviceId);
            
            _deviceId = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister device: {DeviceId}", _deviceId);
        }
    }
    
    /// <summary>
    /// Sends a command to a target device
    /// </summary>
    public async Task SendCommandAsync(ServerCommandMessage command)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            await _hubConnection.InvokeAsync("SendCommand", command);
            
            _logger.LogInformation("Command sent: {CommandId} to device {DeviceId}",
                command.CommandId, command.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command: {CommandId}", command.CommandId);
            throw;
        }
    }
    
    /// <summary>
    /// Sends a print command
    /// </summary>
    public async Task<string> SendPrintCommandAsync(string deviceId, string printData)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        var commandId = Guid.NewGuid().ToString();
        
        try
        {
            await _hubConnection.InvokeAsync("SendPrintCommand", deviceId, printData);
            
            _logger.LogInformation("Print command sent: {CommandId} to device {DeviceId}",
                commandId, deviceId);
            
            return commandId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send print command to device: {DeviceId}", deviceId);
            throw;
        }
    }
    
    /// <summary>
    /// Sends a cash drawer open command
    /// </summary>
    public async Task<string> SendCashDrawerCommandAsync(string deviceId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        var commandId = Guid.NewGuid().ToString();
        
        try
        {
            await _hubConnection.InvokeAsync("SendCashDrawerCommand", deviceId);
            
            _logger.LogInformation("Cash drawer command sent: {CommandId} to device {DeviceId}",
                commandId, deviceId);
            
            return commandId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cash drawer command to device: {DeviceId}", deviceId);
            throw;
        }
    }
    
    /// <summary>
    /// Notifies that a command has been completed
    /// </summary>
    public async Task NotifyCommandCompletedAsync(string commandId, string? result = null)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            await _hubConnection.InvokeAsync("NotifyCommandCompleted", commandId, result);
            
            _logger.LogInformation("Command completion notified: {CommandId}", commandId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify command completion: {CommandId}", commandId);
            throw;
        }
    }
    
    /// <summary>
    /// Notifies that a command has failed
    /// </summary>
    public async Task NotifyCommandFailedAsync(string commandId, string errorMessage)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            await _hubConnection.InvokeAsync("NotifyCommandFailed", commandId, errorMessage);
            
            _logger.LogWarning("Command failure notified: {CommandId} - {Error}",
                commandId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify command failure: {CommandId}", commandId);
            throw;
        }
    }
    
    /// <summary>
    /// Gets the status of a specific command
    /// </summary>
    public async Task<ServerCommandStatus?> GetCommandStatusAsync(string commandId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            var result = await _hubConnection.InvokeAsync<object>("GetCommandStatus", commandId);
            
            // Deserialize the result
            var json = JsonSerializer.Serialize(result);
            var status = JsonSerializer.Deserialize<ServerCommandStatus>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get command status: {CommandId}", commandId);
            throw;
        }
    }
    
    /// <summary>
    /// Gets all pending commands for the current device
    /// </summary>
    public async Task<List<ServerCommandMessage>> GetPendingCommandsAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        if (string.IsNullOrEmpty(_deviceId))
        {
            throw new InvalidOperationException("Device not registered");
        }
        
        try
        {
            var commands = await _hubConnection.InvokeAsync<List<ServerCommandMessage>>(
                "GetPendingCommands", _deviceId);
            
            _logger.LogInformation("Retrieved {Count} pending commands for device {DeviceId}",
                commands.Count, _deviceId);
            
            return commands;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending commands for device: {DeviceId}", _deviceId);
            throw;
        }
    }
    
    /// <summary>
    /// Gets list of registered devices
    /// </summary>
    public async Task<List<RegisteredDevice>> GetRegisteredDevicesAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to ServerCommandHub");
        }
        
        try
        {
            var result = await _hubConnection.InvokeAsync<List<object>>("GetRegisteredDevices");
            
            // Deserialize the result
            var json = JsonSerializer.Serialize(result);
            var devices = JsonSerializer.Deserialize<List<RegisteredDevice>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return devices ?? new List<RegisteredDevice>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get registered devices");
            throw;
        }
    }
    
    // Event handlers
    
    private Task OnConnectionClosed(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "ServerCommandHub connection closed with error");
        }
        else
        {
            _logger.LogInformation("ServerCommandHub connection closed");
        }
        
        ConnectionStateChanged?.Invoke(this, false);
        
        return Task.CompletedTask;
    }
    
    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(exception, "ServerCommandHub reconnecting...");
        
        ConnectionStateChanged?.Invoke(this, false);
        
        return Task.CompletedTask;
    }
    
    private async Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("ServerCommandHub reconnected: {ConnectionId}", connectionId);
        
        // Re-register device if it was registered before
        if (!string.IsNullOrEmpty(_deviceId))
        {
            try
            {
                await RegisterDeviceAsync(_deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-register device after reconnection: {DeviceId}", _deviceId);
            }
        }
        
        ConnectionStateChanged?.Invoke(this, true);
    }
    
    private void OnCommandReceived(ServerCommandMessage command)
    {
        _logger.LogInformation("Command received: {CommandId} - {CommandType}",
            command.CommandId, command.CommandType);
        
        CommandReceived?.Invoke(this, command);
    }
    
    private void OnCommandCompletedMessage(object message)
    {
        try
        {
            // Deserialize the message
            var json = JsonSerializer.Serialize(message);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (data != null)
            {
                var args = new ServerCommandCompletedEventArgs
                {
                    CommandId = data.GetValueOrDefault("CommandId")?.ToString() ?? string.Empty,
                    DeviceId = data.GetValueOrDefault("DeviceId")?.ToString() ?? string.Empty,
                    Result = data.GetValueOrDefault("Result")?.ToString(),
                    CompletedAt = data.ContainsKey("CompletedAt") && data["CompletedAt"] != null
                        ? DateTime.Parse(data["CompletedAt"].ToString()!)
                        : DateTime.UtcNow
                };
                
                _logger.LogInformation("Command completed: {CommandId}", args.CommandId);
                
                CommandCompleted?.Invoke(this, args);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command completed message");
        }
    }
    
    private void OnCommandFailedMessage(ServerCommandMessage command)
    {
        _logger.LogWarning("Command failed: {CommandId} - {Error}",
            command.CommandId, command.ErrorMessage);
        
        CommandFailed?.Invoke(this, command);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing ServerCommandService");
            }
        }
    }
}
