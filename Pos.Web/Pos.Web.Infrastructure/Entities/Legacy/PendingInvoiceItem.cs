using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PendingInvoiceItem
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("PendingInvoiceID")]
    public int PendingInvoiceId { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    public bool IsRefactored { get; set; }

    public bool IsNegativeValue { get; set; }

    [StringLength(100)]
    public string? Note { get; set; }

    [Precision(2)]
    public DateTime? DbTimeStamp { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("PendingInvoiceItems")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;

    [ForeignKey("PendingInvoiceId")]
    [InverseProperty("PendingInvoiceItems")]
    public virtual PendingInvoice PendingInvoice { get; set; } = null!;

    [InverseProperty("PendingInvoiceItem")]
    public virtual ICollection<PendingInvoiceItemsFlavor> PendingInvoiceItemsFlavors { get; set; } = new List<PendingInvoiceItemsFlavor>();
}
