using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class LogType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [InverseProperty("LogType")]
    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
}
