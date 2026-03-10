using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ShopMap
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public DateTime CreationTimestamp { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ShopMapDesignJson { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastDeactivationTimestamp { get; set; }
}
