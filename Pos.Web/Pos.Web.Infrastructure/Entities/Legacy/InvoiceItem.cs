using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoiceItem
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("ParentItemID")]
    public long? ParentItemId { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    public bool IsNegativeValue { get; set; }

    [StringLength(100)]
    public string? Note { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? SalePrice { get; set; }

    [Column("ErpCategoryItemRecipeID")]
    public int? ErpCategoryItemRecipeId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? ItemCost { get; set; }

    /// <summary>
    /// Price includes InvoicesItemsExtras sale price.
    /// </summary>
    [Column(TypeName = "decimal(9, 2)")]
    public decimal? TotalSalePrice { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("InvoiceItems")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;

    [ForeignKey("ErpCategoryItemRecipeId")]
    [InverseProperty("InvoiceItems")]
    public virtual ErpCategoryItemRecipe? ErpCategoryItemRecipe { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceItems")]
    public virtual Invoice Invoice { get; set; } = null!;

    [InverseProperty("InvoiceItem")]
    public virtual ICollection<InvoiceItemExtra> InvoiceItemExtras { get; set; } = new List<InvoiceItemExtra>();

    [InverseProperty("InvoiceItem")]
    public virtual ICollection<InvoiceItemFlavor> InvoiceItemFlavors { get; set; } = new List<InvoiceItemFlavor>();
}
