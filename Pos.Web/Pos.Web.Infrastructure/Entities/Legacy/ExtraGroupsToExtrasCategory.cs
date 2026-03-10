using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ExtraGroupsToExtrasCategory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ExtraCategoryID")]
    public int ExtraCategoryId { get; set; }

    [Column("ExtraGroupID")]
    public int ExtraGroupId { get; set; }

    public int DisplayOrder { get; set; }

    [Column("ColorTypeID")]
    public byte ColorTypeId { get; set; }

    public bool IsSelectionFromGroupRequired { get; set; }

    public bool IsSingleSelectionOnly { get; set; }

    public int? SelectionsMaximumNumber { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("ColorTypeId")]
    [InverseProperty("ExtraGroupsToExtrasCategories")]
    public virtual ColorsType ColorType { get; set; } = null!;

    [ForeignKey("ExtraCategoryId")]
    [InverseProperty("ExtraGroupsToExtrasCategories")]
    public virtual Category ExtraCategory { get; set; } = null!;

    [ForeignKey("ExtraGroupId")]
    [InverseProperty("ExtraGroupsToExtrasCategories")]
    public virtual ExtrasGroup ExtraGroup { get; set; } = null!;
}
