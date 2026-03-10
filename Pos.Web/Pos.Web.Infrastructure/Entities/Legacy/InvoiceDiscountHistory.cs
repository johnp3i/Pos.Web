using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("InvoiceDiscountHistory")]
public partial class InvoiceDiscountHistory
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal GrantTotal { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal DiscountApplied { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal DiscountSavedAmount { get; set; }

    public bool IsPercentage { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal FreeItemsAmount { get; set; }

    [StringLength(300)]
    public string? DiscountDescription { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceDiscountHistory")]
    public virtual Invoice Invoice { get; set; } = null!;
}
