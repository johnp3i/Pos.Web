using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class FlavorGroup
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public bool IsFlavorShownOnTheReceipt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("FlavorGroups")]
    public virtual ICollection<CategoryItemsToFlavorGroup> CategoryItemsToFlavorGroups { get; set; } = new List<CategoryItemsToFlavorGroup>();

    [ForeignKey("FlavorGroupId")]
    [InverseProperty("FlavorGroups")]
    public virtual ICollection<Flavor> Flavors { get; set; } = new List<Flavor>();
}
