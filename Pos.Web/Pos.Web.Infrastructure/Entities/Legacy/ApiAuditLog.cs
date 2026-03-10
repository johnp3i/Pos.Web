using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ApiAuditLog", Schema = "web")]
[Index("Action", "Timestamp", Name = "IX_ApiAuditLog_Action_Timestamp", IsDescending = new[] { false, true })]
[Index("Timestamp", Name = "IX_ApiAuditLog_Timestamp", AllDescending = true)]
[Index("UserId", "Timestamp", Name = "IX_ApiAuditLog_UserID_Timestamp", IsDescending = new[] { false, true })]
public partial class ApiAuditLog
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    public DateTime Timestamp { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [StringLength(100)]
    public string Action { get; set; } = null!;

    [StringLength(100)]
    public string? EntityType { get; set; }

    [Column("EntityID")]
    public int? EntityId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [Column("IPAddress")]
    [StringLength(50)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(500)]
    public string? RequestPath { get; set; }

    [StringLength(10)]
    public string? RequestMethod { get; set; }

    public int? StatusCode { get; set; }

    public int? Duration { get; set; }

    public string? ErrorMessage { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ApiAuditLogs")]
    public virtual User? User { get; set; }
}
