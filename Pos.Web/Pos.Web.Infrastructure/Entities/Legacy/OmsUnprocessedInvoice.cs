using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class OmsUnprocessedInvoice
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    public bool IsPending { get; set; }

    public bool IsProcessed { get; set; }

    public int NumberOfTries { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NextProcessedOn { get; set; }

    public bool IsForcedDeleted { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsUpdated { get; set; }

    public string? ErrorData { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ProcessedTimestamp { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("OmsUnprocessedInvoices")]
    public virtual Invoice Invoice { get; set; } = null!;
}
