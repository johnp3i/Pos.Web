using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ExtrasToGroup
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ExtraGroupID")]
    public int ExtraGroupId { get; set; }

    [Column("ExtraCategoryItemID")]
    public int ExtraCategoryItemId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("ExtraCategoryItemId")]
    [InverseProperty("ExtrasToGroups")]
    public virtual CategoryItem ExtraCategoryItem { get; set; } = null!;

    [ForeignKey("ExtraGroupId")]
    [InverseProperty("ExtrasToGroups")]
    public virtual ExtrasGroup ExtraGroup { get; set; } = null!;
}
