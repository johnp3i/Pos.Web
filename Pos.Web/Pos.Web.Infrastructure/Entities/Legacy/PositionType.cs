using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PositionType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(100)]
    public string Position { get; set; } = null!;

    [StringLength(100)]
    public string? PreviewTextValue { get; set; }

    [InverseProperty("PositionType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
