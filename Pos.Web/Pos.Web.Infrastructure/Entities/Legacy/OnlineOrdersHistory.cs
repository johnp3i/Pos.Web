using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("OnlineOrdersHistory")]
public partial class OnlineOrdersHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    public int OrderNumber { get; set; }

    [Column("OrderID")]
    public long? OrderId { get; set; }

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("OnlineOrdersHistories")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;
}
