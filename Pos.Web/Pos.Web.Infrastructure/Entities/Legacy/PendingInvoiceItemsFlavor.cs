using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PendingInvoiceItemsFlavor
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("PendingInvoiceItemID")]
    public long PendingInvoiceItemId { get; set; }

    [Column("FlavorID")]
    public short FlavorId { get; set; }

    [ForeignKey("FlavorId")]
    [InverseProperty("PendingInvoiceItemsFlavors")]
    public virtual Flavor Flavor { get; set; } = null!;

    [ForeignKey("PendingInvoiceItemId")]
    [InverseProperty("PendingInvoiceItemsFlavors")]
    public virtual PendingInvoiceItem PendingInvoiceItem { get; set; } = null!;
}
