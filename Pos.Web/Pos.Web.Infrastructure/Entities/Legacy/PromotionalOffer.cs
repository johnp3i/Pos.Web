using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PromotionalOffer
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime OfferExpirationDate { get; set; }

    [StringLength(50)]
    public string DisplayName { get; set; } = null!;

    public int OfferValidityDateCounter { get; set; }

    public bool IsAllDaysOffer { get; set; }

    public string? OfferExactDatesAvailability { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal MinOrderAmount { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? MaxOrderAmountVoucherApplied { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal DiscountAmount { get; set; }

    public bool IsPercentage { get; set; }

    [StringLength(200)]
    public string AvailableServicesJson { get; set; } = null!;

    public bool IsAllShopInOffer { get; set; }

    public string? VoucherAcceptedItemsJson { get; set; }

    [Column("RegulationsTemplateID")]
    public int RegulationsTemplateId { get; set; }

    public string? ExtraRegulationsText { get; set; }

    [StringLength(40)]
    public string OfferDescription { get; set; } = null!;

    [StringLength(50)]
    public string? PromoCode { get; set; }

    [StringLength(30)]
    public string? EmphasizeText { get; set; }

    public string? ImageUri { get; set; }

    [StringLength(50)]
    public string Wishes { get; set; } = null!;

    public bool IsActive { get; set; }

    [ForeignKey("RegulationsTemplateId")]
    [InverseProperty("PromotionalOffers")]
    public virtual RegulationsTemplate RegulationsTemplate { get; set; } = null!;

    [InverseProperty("PromotionalOffer")]
    public virtual ICollection<VouchersRelease> VouchersReleases { get; set; } = new List<VouchersRelease>();
}
