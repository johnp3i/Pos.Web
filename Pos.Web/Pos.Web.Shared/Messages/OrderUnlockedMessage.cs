namespace Pos.Web.Shared.Messages;

/// <summary>
/// SignalR message for order lock release
/// </summary>
public class OrderUnlockedMessage
{
    /// <summary>
    /// Order ID that was unlocked
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// User ID who released the lock
    /// </summary>
    public int UnlockedBy { get; set; }
    
    /// <summary>
    /// User name who released the lock
    /// </summary>
    public string UnlockedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of the lock release
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
