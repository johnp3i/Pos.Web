using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("Erp.CategoryItemRecipes")]
public partial class ErpCategoryItemRecipe
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime EnrollmentDate { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ManualRecipeCost { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("ErpCategoryItemRecipes")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;

    [InverseProperty("CategoryItemRecipe")]
    public virtual ICollection<ErpCategoryItemRecipeIngredient> ErpCategoryItemRecipeIngredients { get; set; } = new List<ErpCategoryItemRecipeIngredient>();

    [InverseProperty("ErpCategoryItemRecipe")]
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    [ForeignKey("UserId")]
    [InverseProperty("ErpCategoryItemRecipes")]
    public virtual User User { get; set; } = null!;
}
