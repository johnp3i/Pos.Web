using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("InvoicesPaymentActionHistory")]
public partial class InvoicesPaymentActionHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    [Precision(0)]
    public DateTime Timestamp { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    public bool IsSetAsPaid { get; set; }

    [Column("PaymentTypeID")]
    public byte? PaymentTypeId { get; set; }

    /// <summary>
    /// Set to true when a batch insert is performed (eg. Voucher reconciliation)
    /// </summary>
    public bool IsBatchAction { get; set; }

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("InvoicesPaymentActionHistories")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoicesPaymentActionHistories")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("InvoicesPaymentActionHistories")]
    public virtual PaymentType? PaymentType { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("InvoicesPaymentActionHistories")]
    public virtual User User { get; set; } = null!;
}
