using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CategoryItemsToShopifyProduct
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    [Column("CategoryItemExtraID")]
    public int? CategoryItemExtraId { get; set; }

    [Column("ShopifyProductID")]
    public long ShopifyProductId { get; set; }

    [Column("ShopifyVariantID")]
    public long? ShopifyVariantId { get; set; }

    [Column("NewShopifyVariantID")]
    public long? NewShopifyVariantId { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("CategoryItemsToShopifyProductCategoryItems")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;

    [ForeignKey("CategoryItemExtraId")]
    [InverseProperty("CategoryItemsToShopifyProductCategoryItemExtras")]
    public virtual CategoryItem? CategoryItemExtra { get; set; }
}
