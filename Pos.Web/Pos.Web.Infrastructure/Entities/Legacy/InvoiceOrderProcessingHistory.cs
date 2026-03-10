using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("InvoiceOrderProcessingHistory")]
public partial class InvoiceOrderProcessingHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("OrderServiceTypeOperatorID")]
    public byte? OrderServiceTypeOperatorId { get; set; }

    [Precision(2)]
    public DateTime OrderLeftTimestamp { get; set; }

    [Precision(2)]
    public DateTime? OrderDeliveryReturnTimestamp { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceOrderProcessingHistories")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("OrderServiceTypeOperatorId")]
    [InverseProperty("InvoiceOrderProcessingHistories")]
    public virtual ServiceTypeOperator? OrderServiceTypeOperator { get; set; }
}
