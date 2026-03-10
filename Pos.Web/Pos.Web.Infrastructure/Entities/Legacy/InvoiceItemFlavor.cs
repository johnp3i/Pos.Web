using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoiceItemFlavor
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("InvoiceItemID")]
    public long InvoiceItemId { get; set; }

    [Column("FlavorID")]
    public short FlavorId { get; set; }

    [Column("InvoiceItemExtraID")]
    public long? InvoiceItemExtraId { get; set; }

    [ForeignKey("FlavorId")]
    [InverseProperty("InvoiceItemFlavors")]
    public virtual Flavor Flavor { get; set; } = null!;

    [ForeignKey("InvoiceItemId")]
    [InverseProperty("InvoiceItemFlavors")]
    public virtual InvoiceItem InvoiceItem { get; set; } = null!;

    [ForeignKey("InvoiceItemExtraId")]
    [InverseProperty("InvoiceItemFlavors")]
    public virtual InvoiceItemExtra? InvoiceItemExtra { get; set; }
}
