using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a pending (saved) order
/// </summary>
public class PendingOrderDto
{
    /// <summary>
    /// Pending order ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Order data (full order DTO serialized)
    /// </summary>
    [Required]
    public OrderDto Order { get; set; } = new();
    
    /// <summary>
    /// User ID who saved the order
    /// </summary>
    [Required]
    public int SavedBy { get; set; }
    
    /// <summary>
    /// User name who saved the order
    /// </summary>
    public string? SavedByName { get; set; }
    
    /// <summary>
    /// When the order was saved
    /// </summary>
    public DateTime SavedAt { get; set; }
    
    /// <summary>
    /// Reason for saving (optional)
    /// </summary>
    [MaxLength(200)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Lock status
    /// </summary>
    public OrderLockStatus LockStatus { get; set; }
    
    /// <summary>
    /// User ID who has the lock (if locked)
    /// </summary>
    public int? LockedBy { get; set; }
    
    /// <summary>
    /// User name who has the lock (if locked)
    /// </summary>
    public string? LockedByName { get; set; }
    
    /// <summary>
    /// When the lock expires (if locked)
    /// </summary>
    public DateTime? LockExpiresAt { get; set; }
}
