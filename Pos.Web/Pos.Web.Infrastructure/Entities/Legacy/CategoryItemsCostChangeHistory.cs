using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("CategoryItemsCostChangeHistory")]
public partial class CategoryItemsCostChangeHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal CostToChange { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal NewCostValue { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("CategoryItemsCostChangeHistories")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;
}
