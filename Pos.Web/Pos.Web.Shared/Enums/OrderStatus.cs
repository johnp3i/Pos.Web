namespace Pos.Web.Shared.Enums;

/// <summary>
/// Represents the status of an order in the system
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is being created (not yet submitted)
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Order has been submitted and is pending processing
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Order is being prepared in the kitchen
    /// </summary>
    Preparing = 2,
    
    /// <summary>
    /// Order is ready for pickup/delivery
    /// </summary>
    Ready = 3,
    
    /// <summary>
    /// Order has been delivered/served to customer
    /// </summary>
    Delivered = 4,
    
    /// <summary>
    /// Order has been completed and paid
    /// </summary>
    Completed = 5,
    
    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 6
}
