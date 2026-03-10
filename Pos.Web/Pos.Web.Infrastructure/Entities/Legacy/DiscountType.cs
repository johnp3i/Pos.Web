using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class DiscountType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [InverseProperty("DiscountType")]
    public virtual ICollection<CustomerDiscountHistory> CustomerDiscountHistories { get; set; } = new List<CustomerDiscountHistory>();
}
