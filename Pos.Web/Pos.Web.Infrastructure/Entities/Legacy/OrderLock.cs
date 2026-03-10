using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("OrderLocks", Schema = "web")]
[Index("OrderId", "IsActive", Name = "IX_OrderLocks_OrderID_IsActive")]
public partial class OrderLock
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    public DateTime LockAcquiredAt { get; set; }

    public DateTime LockExpiresAt { get; set; }

    public bool IsActive { get; set; }

    [Column("SessionID")]
    [StringLength(100)]
    public string? SessionId { get; set; }

    [StringLength(200)]
    public string? DeviceInfo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderLocks")]
    public virtual PendingInvoice Order { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("OrderLocks")]
    public virtual User User { get; set; } = null!;
}
