using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Messages;

/// <summary>
/// SignalR message for server commands (device-to-master communication)
/// </summary>
public class ServerCommandMessage
{
    /// <summary>
    /// Command ID
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Command type
    /// </summary>
    public ServerCommandType CommandType { get; set; }
    
    /// <summary>
    /// Device ID that sent the command
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID who initiated the command
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Command payload (JSON data specific to command type)
    /// </summary>
    public string Payload { get; set; } = string.Empty;
    
    /// <summary>
    /// Command status (Queued, Processing, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = "Queued";
    
    /// <summary>
    /// Error message (if failed)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp when command was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when command was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
