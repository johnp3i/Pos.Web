using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CategoryItem
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ItemOnlineID")]
    public long? ItemOnlineId { get; set; }

    [Column("ItemReferenceID")]
    public long? ItemReferenceId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("ExtrasCategoryID")]
    public int? ExtrasCategoryId { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Cost { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ProductVolumeInGr { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ProductTotalWeightInGr { get; set; }

    public string? ImagePath { get; set; }

    public bool IsFreeDrinkApplied { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsExtraItem { get; set; }

    [Column("ColorTypeID")]
    public byte? ColorTypeId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal RealCost { get; set; }

    [StringLength(8)]
    public string? LabelCode { get; set; }

    [Column("LabelHeaderID")]
    public byte? LabelHeaderId { get; set; }

    [Column("VATID")]
    public byte? Vatid { get; set; }

    public string? Summary { get; set; }

    public bool HasPrariorityOnReceipt { get; set; }

    public bool IsOnlineItemOnly { get; set; }

    [Column("ExtrasSpecialFeaturesTypeID")]
    public int? ExtrasSpecialFeaturesTypeId { get; set; }

    [Column("SupplierProductID")]
    public int? SupplierProductId { get; set; }

    public bool IsStandAloneItem { get; set; }

    [Precision(0)]
    public DateTime? LastUpdate { get; set; }

    public bool IsExtraWindowForcedDisabled { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("CategoryItemCategories")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("CategoryItem")]
    public virtual ICollection<CategoryItemsCostChangeHistory> CategoryItemsCostChangeHistories { get; set; } = new List<CategoryItemsCostChangeHistory>();

    [InverseProperty("CategoryItem")]
    public virtual ICollection<CategoryItemsShortcutToolbar> CategoryItemsShortcutToolbars { get; set; } = new List<CategoryItemsShortcutToolbar>();

    [InverseProperty("CategoryItem")]
    public virtual CategoryItemsToFlavorGroup? CategoryItemsToFlavorGroup { get; set; }

    [InverseProperty("CategoryItemExtra")]
    public virtual ICollection<CategoryItemsToShopifyProduct> CategoryItemsToShopifyProductCategoryItemExtras { get; set; } = new List<CategoryItemsToShopifyProduct>();

    [InverseProperty("CategoryItem")]
    public virtual ICollection<CategoryItemsToShopifyProduct> CategoryItemsToShopifyProductCategoryItems { get; set; } = new List<CategoryItemsToShopifyProduct>();

    [ForeignKey("ColorTypeId")]
    [InverseProperty("CategoryItems")]
    public virtual ColorsType? ColorType { get; set; }

    [InverseProperty("CategoryItem")]
    public virtual ICollection<ErpCategoryItemRecipe> ErpCategoryItemRecipes { get; set; } = new List<ErpCategoryItemRecipe>();

    [ForeignKey("ExtrasCategoryId")]
    [InverseProperty("CategoryItemExtrasCategories")]
    public virtual Category? ExtrasCategory { get; set; }

    [ForeignKey("ExtrasSpecialFeaturesTypeId")]
    [InverseProperty("CategoryItems")]
    public virtual ExtrasSpecialFeaturesType? ExtrasSpecialFeaturesType { get; set; }

    [InverseProperty("ExtraCategoryItem")]
    public virtual ICollection<ExtrasToGroup> ExtrasToGroups { get; set; } = new List<ExtrasToGroup>();

    [InverseProperty("CategoryItemExtra")]
    public virtual ICollection<InvoiceItemExtra> InvoiceItemExtras { get; set; } = new List<InvoiceItemExtra>();

    [InverseProperty("CategoryItem")]
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    [ForeignKey("LabelHeaderId")]
    [InverseProperty("CategoryItems")]
    public virtual LabelHeader? LabelHeader { get; set; }

    [InverseProperty("CategoryItem")]
    public virtual ICollection<PendingInvoiceItem> PendingInvoiceItems { get; set; } = new List<PendingInvoiceItem>();

    [ForeignKey("Vatid")]
    [InverseProperty("CategoryItems")]
    public virtual Vat? Vat { get; set; }
}
