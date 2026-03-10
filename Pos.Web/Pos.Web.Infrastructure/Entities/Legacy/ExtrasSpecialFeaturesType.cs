using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ExtrasSpecialFeaturesType")]
public partial class ExtrasSpecialFeaturesType
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    public string Description { get; set; } = null!;

    [InverseProperty("ExtrasSpecialFeaturesType")]
    public virtual ICollection<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();
}
