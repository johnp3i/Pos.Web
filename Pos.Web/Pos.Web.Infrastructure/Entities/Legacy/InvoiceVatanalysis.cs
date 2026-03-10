using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("InvoiceVATAnalysis")]
public partial class InvoiceVatanalysis
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("VATID")]
    public byte Vatid { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? GrossAmount { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceVatanalyses")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("Vatid")]
    [InverseProperty("InvoiceVatanalyses")]
    public virtual Vat Vat { get; set; } = null!;
}
