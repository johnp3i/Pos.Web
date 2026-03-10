using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("InvoicesStockProcessingHistory")]
public partial class InvoicesStockProcessingHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    public bool IsInvoiceDeleted { get; set; }

    public bool IsProcessed { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ProcessingTimestamp { get; set; }

    [Column("SupplierProductsStockUpdateHistoryID")]
    public int? SupplierProductsStockUpdateHistoryId { get; set; }

    [Column("CategoryItemStockUpdateHistoryID")]
    public int? CategoryItemStockUpdateHistoryId { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoicesStockProcessingHistories")]
    public virtual Invoice Invoice { get; set; } = null!;
}
