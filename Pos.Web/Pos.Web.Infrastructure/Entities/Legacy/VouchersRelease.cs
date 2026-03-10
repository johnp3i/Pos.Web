using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class VouchersRelease
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [Column("PromotionalOfferID")]
    public int PromotionalOfferId { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ValidUntil { get; set; }

    [Column("InvoiceID")]
    public int? InvoiceId { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    public bool? IsCancelled { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("VouchersReleases")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("VouchersReleases")]
    public virtual Invoice? Invoice { get; set; }

    [ForeignKey("PromotionalOfferId")]
    [InverseProperty("VouchersReleases")]
    public virtual PromotionalOffer PromotionalOffer { get; set; } = null!;

    [InverseProperty("VouchersReleases")]
    public virtual ICollection<VouchersUsageHistory> VouchersUsageHistories { get; set; } = new List<VouchersUsageHistory>();
}
