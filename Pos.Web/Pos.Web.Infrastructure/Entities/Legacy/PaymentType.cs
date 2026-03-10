using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PaymentType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string DisplayName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsAllowPartialPayment { get; set; }

    public bool IsDefaultPayment { get; set; }

    public bool IsAmountFullCovered { get; set; }

    public bool IsAmountRedirectedToTerminal { get; set; }

    [InverseProperty("PaymentType")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("PaymentType")]
    public virtual ICollection<InvoicesPaymentActionHistory> InvoicesPaymentActionHistories { get; set; } = new List<InvoicesPaymentActionHistory>();
}
