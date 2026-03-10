using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ZReportTypes")]
public partial class ZreportType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = null!;

    [InverseProperty("ZreportType")]
    public virtual ICollection<ZreportsExport> ZreportsExports { get; set; } = new List<ZreportsExport>();
}
