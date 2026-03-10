using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ColorsType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    public byte DisplayOrder { get; set; }

    [InverseProperty("ColorType")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("ColorType")]
    public virtual ICollection<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();

    [InverseProperty("ColorType")]
    public virtual ICollection<ExtraGroupsToExtrasCategory> ExtraGroupsToExtrasCategories { get; set; } = new List<ExtraGroupsToExtrasCategory>();

    [InverseProperty("ColorType")]
    public virtual ICollection<ServiceTypeOperator> ServiceTypeOperators { get; set; } = new List<ServiceTypeOperator>();

    [InverseProperty("ColorType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
