using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CategoryItemsToFlavorGroup
{
    [Key]
    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    [Column("FlavorGroupsID")]
    public byte FlavorGroupsId { get; set; }

    public bool IsFlavorsRequired { get; set; }

    public bool IsExtraEnabled { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("CategoryItemsToFlavorGroup")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;

    [ForeignKey("FlavorGroupsId")]
    [InverseProperty("CategoryItemsToFlavorGroups")]
    public virtual FlavorGroup FlavorGroups { get; set; } = null!;
}
