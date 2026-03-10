using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Config
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Property { get; set; } = null!;

    public int Value { get; set; }

    public string? StringValue { get; set; }

    public string? Description { get; set; }
}
