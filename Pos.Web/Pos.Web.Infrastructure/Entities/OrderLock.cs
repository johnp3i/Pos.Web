using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Order lock entity to prevent concurrent editing of orders
/// Mapped to web.OrderLocks table
/// </summary>
[Table("OrderLocks", Schema = "web")]
public class OrderLock
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    public int OrderID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public DateTime LockAcquiredAt { get; set; }

    [Required]
    public DateTime LockExpiresAt { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [MaxLength(100)]
    public string? SessionID { get; set; }

    [MaxLength(200)]
    public string? DeviceInfo { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }

    /// <summary>
    /// Checks if the lock is still valid (active and not expired)
    /// </summary>
    [NotMapped]
    public bool IsValid => IsActive && LockExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Gets the remaining time before lock expires
    /// </summary>
    [NotMapped]
    public TimeSpan TimeRemaining => IsValid 
        ? LockExpiresAt - DateTime.UtcNow 
        : TimeSpan.Zero;
}
