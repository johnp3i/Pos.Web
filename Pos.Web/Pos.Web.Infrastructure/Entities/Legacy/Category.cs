using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Category
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    public bool IsActive { get; set; }

    public string? ImagePath { get; set; }

    public int DisplayOrder { get; set; }

    [Column("CategorySectionTypeID")]
    public byte CategorySectionTypeId { get; set; }

    [Column("CategoryExtrasTypeID")]
    public byte? CategoryExtrasTypeId { get; set; }

    public bool HasSeperateReceipt { get; set; }

    public bool IsExtraCategory { get; set; }

    [Column("CategoryOperationDepartmentID")]
    public byte CategoryOperationDepartmentId { get; set; }

    public bool IsExtrasWindowEnable { get; set; }

    public bool IsOnlineCategoryOnly { get; set; }

    [Column("ColorTypeID")]
    public byte? ColorTypeId { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<CategoriesToCategoriesExtra> CategoriesToCategoriesExtraCategories { get; set; } = new List<CategoriesToCategoriesExtra>();

    [InverseProperty("CategoryExtras")]
    public virtual ICollection<CategoriesToCategoriesExtra> CategoriesToCategoriesExtraCategoryExtras { get; set; } = new List<CategoriesToCategoriesExtra>();

    [InverseProperty("Category")]
    public virtual ICollection<CategoryItem> CategoryItemCategories { get; set; } = new List<CategoryItem>();

    [InverseProperty("ExtrasCategory")]
    public virtual ICollection<CategoryItem> CategoryItemExtrasCategories { get; set; } = new List<CategoryItem>();

    [ForeignKey("CategoryOperationDepartmentId")]
    [InverseProperty("Categories")]
    public virtual CategoryOperationDepartment CategoryOperationDepartment { get; set; } = null!;

    [ForeignKey("CategorySectionTypeId")]
    [InverseProperty("Categories")]
    public virtual CategorySectionType CategorySectionType { get; set; } = null!;

    [ForeignKey("ColorTypeId")]
    [InverseProperty("Categories")]
    public virtual ColorsType? ColorType { get; set; }

    [InverseProperty("ExtraCategory")]
    public virtual ICollection<ExtraGroupsToExtrasCategory> ExtraGroupsToExtrasCategories { get; set; } = new List<ExtraGroupsToExtrasCategory>();
}
