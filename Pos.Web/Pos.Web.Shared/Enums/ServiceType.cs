namespace Pos.Web.Shared.Enums;

/// <summary>
/// Represents the type of service for an order
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Dine-in service (eat at restaurant)
    /// </summary>
    DineIn = 1,
    
    /// <summary>
    /// Takeout/Takeaway service
    /// </summary>
    Takeout = 2,
    
    /// <summary>
    /// Delivery service
    /// </summary>
    Delivery = 3,
    
    /// <summary>
    /// Drive-through service
    /// </summary>
    DriveThrough = 4
}
