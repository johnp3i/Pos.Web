using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Invoice
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("ServingTypeToVatID")]
    public byte ServingTypeToVatId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal CostNoVat { get; set; }

    [Column("PaymentTypeID")]
    public byte? PaymentTypeId { get; set; }

    /// <summary>
    /// Price including VAT
    /// </summary>
    [Column(TypeName = "decimal(9, 2)")]
    public decimal TotalCost { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? DeliveryCharge { get; set; }

    /// <summary>
    /// If it is null then the customer has paid the exact invoice amount
    /// </summary>
    [Column(TypeName = "decimal(9, 2)")]
    public decimal? CustomerPaid { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsNegative { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("AddressID")]
    public int? AddressId { get; set; }

    public string? Note { get; set; }

    public bool IsFreeDrinkByPassed { get; set; }

    public bool IsFromExternalDevice { get; set; }

    public bool? IsExternalDeviceRequestLabelPrinting { get; set; }

    public bool? IsExternalRequestProcessed { get; set; }

    [Column("POSDeviceID")]
    public byte? PosdeviceId { get; set; }

    public bool IsItemsWritingCompleted { get; set; }

    public byte? TableNumber { get; set; }

    [Precision(2)]
    public DateTime? OrderScheduledTimestamp { get; set; }

    [Column("ReferenceInvoiceID")]
    public int? ReferenceInvoiceId { get; set; }

    public bool IsOnlineOrder { get; set; }

    public bool IsPrePaid { get; set; }

    public string? AdminNote { get; set; }

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    public bool IsUnpaid { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? PaymentDate { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? CalculatedWeightInKg { get; set; }

    [Column("PendingInvoiceID")]
    public int? PendingInvoiceId { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("Invoices")]
    public virtual Address? Address { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Invoices")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("Invoices")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceDeletionHistory> InvoiceDeletionHistories { get; set; } = new List<InvoiceDeletionHistory>();

    [InverseProperty("Invoice")]
    public virtual InvoiceDiscountHistory? InvoiceDiscountHistory { get; set; }

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceOrderProcessingHistory> InvoiceOrderProcessingHistories { get; set; } = new List<InvoiceOrderProcessingHistory>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceVatanalysis> InvoiceVatanalyses { get; set; } = new List<InvoiceVatanalysis>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoicesPaymentActionHistory> InvoicesPaymentActionHistories { get; set; } = new List<InvoicesPaymentActionHistory>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoicesPaymentType> InvoicesPaymentTypes { get; set; } = new List<InvoicesPaymentType>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoicesSpecialNote> InvoicesSpecialNotes { get; set; } = new List<InvoicesSpecialNote>();

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoicesStockProcessingHistory> InvoicesStockProcessingHistories { get; set; } = new List<InvoicesStockProcessingHistory>();

    [InverseProperty("Invoice")]
    public virtual ICollection<JcctransactionsHistory> JcctransactionsHistories { get; set; } = new List<JcctransactionsHistory>();

    [InverseProperty("Invoice")]
    public virtual ICollection<OmsUnprocessedInvoice> OmsUnprocessedInvoices { get; set; } = new List<OmsUnprocessedInvoice>();

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("Invoices")]
    public virtual PaymentType? PaymentType { get; set; }

    [ForeignKey("PendingInvoiceId")]
    [InverseProperty("Invoices")]
    public virtual PendingInvoice? PendingInvoice { get; set; }

    [ForeignKey("PosdeviceId")]
    [InverseProperty("Invoices")]
    public virtual Posdevice? Posdevice { get; set; }

    [ForeignKey("ServingTypeToVatId")]
    [InverseProperty("Invoices")]
    public virtual ServingTypesToVat ServingTypeToVat { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Invoices")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Invoice")]
    public virtual ICollection<VouchersRelease> VouchersReleases { get; set; } = new List<VouchersRelease>();

    [InverseProperty("UsedByInvoice")]
    public virtual ICollection<VouchersUsageHistory> VouchersUsageHistories { get; set; } = new List<VouchersUsageHistory>();
}
