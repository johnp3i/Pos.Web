using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("JCCPluginsStatusCodes")]
public partial class JccpluginsStatusCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("JCCPluginID")]
    public byte JccpluginId { get; set; }

    [StringLength(2)]
    public string StatusCode { get; set; } = null!;

    public bool IsSucceedResponse { get; set; }

    [StringLength(300)]
    public string DescriptionPrimary { get; set; } = null!;

    [StringLength(300)]
    public string? DescriptionSecondary { get; set; }

    [ForeignKey("JccpluginId")]
    [InverseProperty("JccpluginsStatusCodes")]
    public virtual JccpluginType Jccplugin { get; set; } = null!;
}
