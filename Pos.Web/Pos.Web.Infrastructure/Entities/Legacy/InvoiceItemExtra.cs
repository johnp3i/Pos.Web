using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoiceItemExtra
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("InvoiceItemID")]
    public long InvoiceItemId { get; set; }

    [Column("CategoryItemExtraID")]
    public int CategoryItemExtraId { get; set; }

    [StringLength(150)]
    public string ExtraName { get; set; } = null!;

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? SalePrice { get; set; }

    [Column("ErpCategoryItemRecipeID")]
    public int? ErpCategoryItemRecipeId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? ItemCost { get; set; }

    [ForeignKey("CategoryItemExtraId")]
    [InverseProperty("InvoiceItemExtras")]
    public virtual CategoryItem CategoryItemExtra { get; set; } = null!;

    [ForeignKey("InvoiceItemId")]
    [InverseProperty("InvoiceItemExtras")]
    public virtual InvoiceItem InvoiceItem { get; set; } = null!;

    [InverseProperty("InvoiceItemExtra")]
    public virtual ICollection<InvoiceItemFlavor> InvoiceItemFlavors { get; set; } = new List<InvoiceItemFlavor>();
}
