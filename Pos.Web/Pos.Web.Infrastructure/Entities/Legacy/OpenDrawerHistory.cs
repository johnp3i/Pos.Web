using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("OpenDrawerHistory")]
public partial class OpenDrawerHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("OpenDrawerHistories")]
    public virtual User User { get; set; } = null!;
}
