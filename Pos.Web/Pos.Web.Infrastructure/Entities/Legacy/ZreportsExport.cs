using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ZReportsExports")]
public partial class ZreportsExport
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime Timesatmp { get; set; }

    [Column("ZReportTypeID")]
    public byte ZreportTypeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateFrom { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateTo { get; set; }

    [ForeignKey("ZreportTypeId")]
    [InverseProperty("ZreportsExports")]
    public virtual ZreportType ZreportType { get; set; } = null!;
}
