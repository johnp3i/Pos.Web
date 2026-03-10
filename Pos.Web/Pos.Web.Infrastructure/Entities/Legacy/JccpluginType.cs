using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("JCCPluginTypes")]
public partial class JccpluginType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Description { get; set; } = null!;

    [InverseProperty("Jccplugin")]
    public virtual ICollection<JccpluginsStatusCode> JccpluginsStatusCodes { get; set; } = new List<JccpluginsStatusCode>();

    [InverseProperty("JccpluginType")]
    public virtual ICollection<Posdevice> Posdevices { get; set; } = new List<Posdevice>();
}
