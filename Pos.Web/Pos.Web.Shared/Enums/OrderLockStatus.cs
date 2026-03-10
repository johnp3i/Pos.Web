namespace Pos.Web.Shared.Enums;

/// <summary>
/// Represents the status of an order lock
/// </summary>
public enum OrderLockStatus
{
    /// <summary>
    /// Order is not locked (available for editing)
    /// </summary>
    Unlocked = 0,
    
    /// <summary>
    /// Order is locked by current user
    /// </summary>
    LockedByCurrentUser = 1,
    
    /// <summary>
    /// Order is locked by another user
    /// </summary>
    LockedByOtherUser = 2,
    
    /// <summary>
    /// Lock has expired
    /// </summary>
    Expired = 3
}
