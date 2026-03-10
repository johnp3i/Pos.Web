using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ExtrasGroup
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime EnrollementDate { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("ExtraGroup")]
    public virtual ICollection<ExtraGroupsToExtrasCategory> ExtraGroupsToExtrasCategories { get; set; } = new List<ExtraGroupsToExtrasCategory>();

    [InverseProperty("ExtraGroup")]
    public virtual ICollection<ExtrasToGroup> ExtrasToGroups { get; set; } = new List<ExtrasToGroup>();
}
