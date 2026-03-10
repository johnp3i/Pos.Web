using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pos.Web.Shared.Constants;
using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Messages;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Pos.Web.API.Hubs;

/// <summary>
/// SignalR hub for device-to-master server command communication
/// Handles print commands, cash drawer commands, and other device operations
/// </summary>
[Authorize]
public class ServerCommandHub : Hub
{
    private readonly ILogger<ServerCommandHub> _logger;
    
    // In-memory command tracking (in production, use Redis or database)
    private static readonly ConcurrentDictionary<string, ServerCommandMessage> _commandQueue = new();
    private static readonly ConcurrentDictionary<string, string> _deviceConnections = new();

    public ServerCommandHub(ILogger<ServerCommandHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        _logger.LogInformation("User {UserName} (ID: {UserId}) connected to ServerCommandHub. ConnectionId: {ConnectionId}", 
            userName, userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        // Remove device registration if exists
        var deviceId = _deviceConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (!string.IsNullOrEmpty(deviceId))
        {
            _deviceConnections.TryRemove(deviceId, out _);
            _logger.LogInformation("Device {DeviceId} unregistered on disconnect", deviceId);
        }
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserName} (ID: {UserId}) disconnected from ServerCommandHub with error. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User {UserName} (ID: {UserId}) disconnected from ServerCommandHub. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Register a device as a master station that can receive commands
    /// </summary>
    /// <param name="deviceId">Unique device identifier</param>
    public async Task RegisterDevice(string deviceId)
    {
        try
        {
            var userName = GetUserName();
            
            _deviceConnections.AddOrUpdate(deviceId, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
            
            _logger.LogInformation("Device {DeviceId} registered by user {UserName}. ConnectionId: {ConnectionId}", 
                deviceId, userName, Context.ConnectionId);
            
            await Clients.Caller.SendAsync("DeviceRegistered", new
            {
                DeviceId = deviceId,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device {DeviceId}", deviceId);
            throw;
        }
    }

    /// <summary>
    /// Unregister a device
    /// </summary>
    /// <param name="deviceId">Device identifier to unregister</param>
    public async Task UnregisterDevice(string deviceId)
    {
        try
        {
            _deviceConnections.TryRemove(deviceId, out _);
            
            _logger.LogInformation("Device {DeviceId} unregistered", deviceId);
            
            await Clients.Caller.SendAsync("DeviceUnregistered", new
            {
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device {DeviceId}", deviceId);
            throw;
        }
    }

    /// <summary>
    /// Send a command from a client device to the master station
    /// </summary>
    /// <param name="command">Server command message</param>
    public async Task SendCommand(ServerCommandMessage command)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            
            // Set command metadata
            command.UserId = userId;
            command.CreatedAt = DateTime.UtcNow;
            command.Status = "Queued";
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) sending {CommandType} command {CommandId} from device {DeviceId}", 
                userName, userId, command.CommandType, command.CommandId, command.DeviceId);
            
            // Store command in queue
            _commandQueue.TryAdd(command.CommandId, command);
            
            // Find target device connection
            if (_deviceConnections.TryGetValue(command.DeviceId, out var connectionId))
            {
                // Send command to specific device
                await Clients.Client(connectionId).SendAsync(
                    SignalRMethods.ServerCommand.CommandReceived, 
                    command);
                
                _logger.LogInformation("Command {CommandId} sent to device {DeviceId}", 
                    command.CommandId, command.DeviceId);
            }
            else
            {
                // Device not connected, mark command as failed
                command.Status = "Failed";
                command.ErrorMessage = $"Device {command.DeviceId} is not connected";
                
                _logger.LogWarning("Command {CommandId} failed: Device {DeviceId} not connected", 
                    command.CommandId, command.DeviceId);
                
                // Notify sender of failure
                await Clients.Caller.SendAsync(SignalRMethods.ServerCommand.CommandFailed, command);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command {CommandId}", command.CommandId);
            
            command.Status = "Failed";
            command.ErrorMessage = "An error occurred while sending command";
            
            await Clients.Caller.SendAsync(SignalRMethods.ServerCommand.CommandFailed, command);
        }
    }

    /// <summary>
    /// Send a print command to a specific device
    /// </summary>
    /// <param name="deviceId">Target device ID</param>
    /// <param name="printData">Print data (receipt content, format, etc.)</param>
    public async Task SendPrintCommand(string deviceId, string printData)
    {
        var command = new ServerCommandMessage
        {
            CommandId = Guid.NewGuid().ToString(),
            CommandType = ServerCommandType.PrintReceipt,
            DeviceId = deviceId,
            Payload = printData,
            UserId = GetUserId()
        };
        
        await SendCommand(command);
    }

    /// <summary>
    /// Send a cash drawer open command to a specific device
    /// </summary>
    /// <param name="deviceId">Target device ID</param>
    public async Task SendCashDrawerCommand(string deviceId)
    {
        var command = new ServerCommandMessage
        {
            CommandId = Guid.NewGuid().ToString(),
            CommandType = ServerCommandType.OpenCashDrawer,
            DeviceId = deviceId,
            Payload = string.Empty,
            UserId = GetUserId()
        };
        
        await SendCommand(command);
    }

    /// <summary>
    /// Notify that a command has been completed by the device
    /// </summary>
    /// <param name="commandId">Command ID that was completed</param>
    /// <param name="result">Result data (optional)</param>
    public async Task NotifyCommandCompleted(string commandId, string? result = null)
    {
        try
        {
            if (_commandQueue.TryGetValue(commandId, out var command))
            {
                command.Status = "Completed";
                command.CompletedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Command {CommandId} completed by device {DeviceId}", 
                    commandId, command.DeviceId);
                
                // Notify all clients that command completed
                await Clients.All.SendAsync(SignalRMethods.ServerCommand.CommandCompleted, new
                {
                    CommandId = commandId,
                    DeviceId = command.DeviceId,
                    Result = result,
                    CompletedAt = command.CompletedAt
                });
                
                // Remove from queue after a delay (for status checking)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    _commandQueue.TryRemove(commandId, out _);
                });
            }
            else
            {
                _logger.LogWarning("Command {CommandId} not found in queue", commandId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying command completion for {CommandId}", commandId);
        }
    }

    /// <summary>
    /// Notify that a command has failed on the device
    /// </summary>
    /// <param name="commandId">Command ID that failed</param>
    /// <param name="errorMessage">Error message</param>
    public async Task NotifyCommandFailed(string commandId, string errorMessage)
    {
        try
        {
            if (_commandQueue.TryGetValue(commandId, out var command))
            {
                command.Status = "Failed";
                command.ErrorMessage = errorMessage;
                command.CompletedAt = DateTime.UtcNow;
                
                _logger.LogWarning("Command {CommandId} failed on device {DeviceId}: {Error}", 
                    commandId, command.DeviceId, errorMessage);
                
                // Notify all clients that command failed
                await Clients.All.SendAsync(SignalRMethods.ServerCommand.CommandFailed, command);
                
                // Remove from queue after a delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    _commandQueue.TryRemove(commandId, out _);
                });
            }
            else
            {
                _logger.LogWarning("Command {CommandId} not found in queue", commandId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying command failure for {CommandId}", commandId);
        }
    }

    /// <summary>
    /// Get the status of a specific command
    /// </summary>
    /// <param name="commandId">Command ID to check</param>
    /// <returns>Command status information</returns>
    public Task<object> GetCommandStatus(string commandId)
    {
        try
        {
            if (_commandQueue.TryGetValue(commandId, out var command))
            {
                return Task.FromResult<object>(new
                {
                    CommandId = command.CommandId,
                    CommandType = command.CommandType,
                    DeviceId = command.DeviceId,
                    Status = command.Status,
                    ErrorMessage = command.ErrorMessage,
                    CreatedAt = command.CreatedAt,
                    CompletedAt = command.CompletedAt
                });
            }
            
            return Task.FromResult<object>(new
            {
                CommandId = commandId,
                Status = "NotFound",
                ErrorMessage = "Command not found in queue"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting command status for {CommandId}", commandId);
            throw;
        }
    }

    /// <summary>
    /// Get all pending commands for a specific device
    /// </summary>
    /// <param name="deviceId">Device ID to check</param>
    /// <returns>List of pending commands</returns>
    public Task<List<ServerCommandMessage>> GetPendingCommands(string deviceId)
    {
        try
        {
            var pendingCommands = _commandQueue.Values
                .Where(c => c.DeviceId == deviceId && c.Status == "Queued")
                .OrderBy(c => c.CreatedAt)
                .ToList();
            
            _logger.LogInformation("Retrieved {Count} pending commands for device {DeviceId}", 
                pendingCommands.Count, deviceId);
            
            return Task.FromResult(pendingCommands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending commands for device {DeviceId}", deviceId);
            throw;
        }
    }

    /// <summary>
    /// Get list of registered devices
    /// </summary>
    /// <returns>List of device IDs and their connection status</returns>
    public Task<List<object>> GetRegisteredDevices()
    {
        try
        {
            var devices = _deviceConnections.Select(kvp => new
            {
                DeviceId = kvp.Key,
                ConnectionId = kvp.Value,
                IsConnected = true
            }).ToList<object>();
            
            _logger.LogInformation("Retrieved {Count} registered devices", devices.Count);
            
            return Task.FromResult(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registered devices");
            throw;
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get current user name from claims
    /// </summary>
    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }
}
