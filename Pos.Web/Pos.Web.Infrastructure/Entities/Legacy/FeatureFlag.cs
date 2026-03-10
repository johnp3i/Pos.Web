using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("FeatureFlags", Schema = "web")]
[Index("IsEnabled", Name = "IX_FeatureFlags_IsEnabled")]
[Index("Name", Name = "UQ_FeatureFlags_Name", IsUnique = true)]
public partial class FeatureFlag
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; }

    [Column("EnabledForUserIDs")]
    public string? EnabledForUserIds { get; set; }

    public string? EnabledForRoles { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("FeatureFlags")]
    public virtual User? UpdatedByNavigation { get; set; }
}
