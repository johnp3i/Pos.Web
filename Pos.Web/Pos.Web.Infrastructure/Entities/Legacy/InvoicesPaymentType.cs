using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoicesPaymentType
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("PaymentTypeID")]
    public byte PaymentTypeId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal PaymentAmount { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal InvoiceTotalAmount { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoicesPaymentTypes")]
    public virtual Invoice Invoice { get; set; } = null!;
}
