using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Flavor
{
    [Key]
    [Column("ID")]
    public short Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(8)]
    public string? LabelCode { get; set; }

    [Column("LabelHeaderID")]
    public byte? LabelHeaderId { get; set; }

    public bool IsEnabled { get; set; }

    [InverseProperty("Flavor")]
    public virtual ICollection<InvoiceItemFlavor> InvoiceItemFlavors { get; set; } = new List<InvoiceItemFlavor>();

    [ForeignKey("LabelHeaderId")]
    [InverseProperty("Flavors")]
    public virtual LabelHeader? LabelHeader { get; set; }

    [InverseProperty("Flavor")]
    public virtual ICollection<PendingInvoiceItemsFlavor> PendingInvoiceItemsFlavors { get; set; } = new List<PendingInvoiceItemsFlavor>();

    [ForeignKey("FlavorId")]
    [InverseProperty("Flavors")]
    public virtual ICollection<FlavorGroup> FlavorGroups { get; set; } = new List<FlavorGroup>();
}
