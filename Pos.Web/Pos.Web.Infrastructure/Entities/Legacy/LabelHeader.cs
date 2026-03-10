using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class LabelHeader
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    public string Header { get; set; } = null!;

    [InverseProperty("LabelHeader")]
    public virtual ICollection<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();

    [InverseProperty("LabelHeader")]
    public virtual ICollection<Flavor> Flavors { get; set; } = new List<Flavor>();
}
