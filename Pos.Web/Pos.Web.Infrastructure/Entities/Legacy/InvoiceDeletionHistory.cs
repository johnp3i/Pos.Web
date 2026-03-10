using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[PrimaryKey("UserId", "InvoiceId")]
[Table("InvoiceDeletionHistory")]
public partial class InvoiceDeletionHistory
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceDeletionHistories")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("InvoiceDeletionHistories")]
    public virtual User User { get; set; } = null!;
}
