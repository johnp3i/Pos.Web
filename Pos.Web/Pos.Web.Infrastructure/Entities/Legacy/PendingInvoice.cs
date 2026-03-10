using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class PendingInvoice
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Precision(2)]
    public DateTime TimeStamp { get; set; }

    [Column("SavedByUserID")]
    public int? SavedByUserId { get; set; }

    public bool IsCheckOut { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Discount { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("AddressID")]
    public int? AddressId { get; set; }

    public bool IsPercentage { get; set; }

    public string? Note { get; set; }

    public bool? IsFromMap { get; set; }

    public bool? IsUnpaid { get; set; }

    public byte? TableNumber { get; set; }

    [Column("MapTableID")]
    public Guid? MapTableId { get; set; }

    [Precision(2)]
    public DateTime? OrderScheduledTimestamp { get; set; }

    [Column("ExportUserID")]
    public int? ExportUserId { get; set; }

    [Column("ServingTypeID")]
    public byte? ServingTypeId { get; set; }

    [Precision(2)]
    public DateTime? LastUpdateTimestamp { get; set; }

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    [Column("POSDeviceID")]
    public byte? PosdeviceId { get; set; }

    public bool IsOnlineOrder { get; set; }

    [Column("ReferenceInvoiceID")]
    public int? ReferenceInvoiceId { get; set; }

    public string? AdminNote { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("PendingInvoices")]
    public virtual Address? Address { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("PendingInvoices")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("PendingInvoices")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;

    [ForeignKey("ExportUserId")]
    [InverseProperty("PendingInvoiceExportUsers")]
    public virtual User? ExportUser { get; set; }

    [InverseProperty("PendingInvoice")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderLock> OrderLocks { get; set; } = new List<OrderLock>();

    [InverseProperty("PendingInvoice")]
    public virtual ICollection<PendingInvoiceItem> PendingInvoiceItems { get; set; } = new List<PendingInvoiceItem>();

    [ForeignKey("PosdeviceId")]
    [InverseProperty("PendingInvoices")]
    public virtual Posdevice? Posdevice { get; set; }

    [ForeignKey("SavedByUserId")]
    [InverseProperty("PendingInvoiceSavedByUsers")]
    public virtual User? SavedByUser { get; set; }

    [ForeignKey("ServingTypeId")]
    [InverseProperty("PendingInvoices")]
    public virtual ServingType? ServingType { get; set; }
}
