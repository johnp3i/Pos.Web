using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("SyncQueue", Schema = "web")]
[Index("Status", "ClientTimestamp", Name = "IX_SyncQueue_Status_ClientTimestamp")]
[Index("UserId", "DeviceId", "Status", Name = "IX_SyncQueue_UserID_DeviceID_Status")]
public partial class SyncQueue
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("DeviceID")]
    [StringLength(100)]
    public string DeviceId { get; set; } = null!;

    [StringLength(50)]
    public string OperationType { get; set; } = null!;

    [StringLength(100)]
    public string EntityType { get; set; } = null!;

    [Column("EntityID")]
    public int? EntityId { get; set; }

    public string Payload { get; set; } = null!;

    public DateTime ClientTimestamp { get; set; }

    public DateTime ServerTimestamp { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public DateTime? LastAttemptAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("SyncQueues")]
    public virtual User User { get; set; } = null!;
}
