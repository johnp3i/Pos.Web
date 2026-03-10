using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Messages;

/// <summary>
/// SignalR message for order status changes
/// </summary>
public class OrderStatusChangedMessage
{
    /// <summary>
    /// Order ID
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// Previous order status
    /// </summary>
    public OrderStatus OldStatus { get; set; }
    
    /// <summary>
    /// New order status
    /// </summary>
    public OrderStatus NewStatus { get; set; }
    
    /// <summary>
    /// User ID who changed the status
    /// </summary>
    public int ChangedBy { get; set; }
    
    /// <summary>
    /// User name who changed the status
    /// </summary>
    public string ChangedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when status was changed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional reason for status change
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}