using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("Erp.CategoryItemRecipeIngredients")]
public partial class ErpCategoryItemRecipeIngredient
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CategoryItemRecipeID")]
    public int CategoryItemRecipeId { get; set; }

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [Column("SupplierProductID")]
    public int SupplierProductId { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal? Volume { get; set; }

    [Column("SupplierProductCostHistoryID")]
    public int? SupplierProductCostHistoryId { get; set; }

    public bool IsCancelled { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CancellationDate { get; set; }

    [ForeignKey("CategoryItemRecipeId")]
    [InverseProperty("ErpCategoryItemRecipeIngredients")]
    public virtual ErpCategoryItemRecipe CategoryItemRecipe { get; set; } = null!;
}
