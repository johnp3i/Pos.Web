using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Messages;

/// <summary>
/// SignalR message for new orders sent to kitchen
/// </summary>
public class KitchenOrderMessage
{
    /// <summary>
    /// Order ID
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// Complete order details
    /// </summary>
    public OrderDto Order { get; set; } = new();
    
    /// <summary>
    /// User ID who sent the order to kitchen
    /// </summary>
    public int SentBy { get; set; }
    
    /// <summary>
    /// User name who sent the order to kitchen
    /// </summary>
    public string SentByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when order was sent to kitchen
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Priority level (Normal, High, Urgent)
    /// </summary>
    public string Priority { get; set; } = "Normal";
    
    /// <summary>
    /// Estimated preparation time in minutes
    /// </summary>
    public int? EstimatedPrepTime { get; set; }
}