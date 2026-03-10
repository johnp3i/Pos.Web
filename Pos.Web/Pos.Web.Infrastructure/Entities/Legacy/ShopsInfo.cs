using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ShopsInfo")]
public partial class ShopsInfo
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string City { get; set; } = null!;

    [Column("ShopID")]
    public byte? ShopId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Telephone { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Address { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ManagerName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ManagerPhone { get; set; }
}
