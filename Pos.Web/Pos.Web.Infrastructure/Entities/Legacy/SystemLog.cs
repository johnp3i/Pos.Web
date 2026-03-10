using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class SystemLog
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("LogTypeID")]
    public byte LogTypeId { get; set; }

    [Unicode(false)]
    public string? Details { get; set; }

    [ForeignKey("LogTypeId")]
    [InverseProperty("SystemLogs")]
    public virtual LogType LogType { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("SystemLogs")]
    public virtual User? User { get; set; }
}
