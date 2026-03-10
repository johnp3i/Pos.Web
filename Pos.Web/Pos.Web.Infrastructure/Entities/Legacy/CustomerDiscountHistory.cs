using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[PrimaryKey("CustomerId", "Timestamp")]
[Table("CustomerDiscountHistory")]
public partial class CustomerDiscountHistory
{
    [Key]
    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Key]
    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? DiscountName { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column("DiscountTypeID")]
    public byte DiscountTypeId { get; set; }

    [Column("InvoiceID")]
    public int? InvoiceId { get; set; }

    public bool IsSynched { get; set; }

    public int? DrinksForDiscount { get; set; }

    public byte? CurrentBuffer { get; set; }

    public byte? NewBuffer { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerDiscountHistories")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("DiscountTypeId")]
    [InverseProperty("CustomerDiscountHistories")]
    public virtual DiscountType DiscountType { get; set; } = null!;
}
