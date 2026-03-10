namespace Pos.Web.Shared.Messages;

/// <summary>
/// SignalR message for order lock acquisition
/// </summary>
public class OrderLockedMessage
{
    /// <summary>
    /// Order ID that was locked
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// User ID who acquired the lock
    /// </summary>
    public int LockedBy { get; set; }
    
    /// <summary>
    /// User name who acquired the lock
    /// </summary>
    public string LockedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// When the lock expires
    /// </summary>
    public DateTime LockExpiresAt { get; set; }
    
    /// <summary>
    /// Timestamp of the lock acquisition
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
