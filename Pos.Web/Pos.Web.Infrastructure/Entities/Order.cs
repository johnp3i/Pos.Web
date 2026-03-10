namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Order entity (placeholder for legacy dbo.Invoices table)
/// This will be mapped to the actual legacy table structure later
/// </summary>
public class Order
{
    public int ID { get; set; }
    public int? CustomerID { get; set; }
    public int UserID { get; set; }
    public byte ServiceTypeID { get; set; }
    public string ServiceType { get; set; } = string.Empty; // Mapped from ServiceTypeID
    public byte? TableNumber { get; set; }
    public string? Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalAmount { get; set; } // Same as TotalCost
    public decimal? CustomerPaid { get; set; }
    public decimal? AmountPaid { get; set; } // Same as CustomerPaid
    public decimal? ChangeAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public int? VoucherID { get; set; }
    public string? InvoiceNote { get; set; }
    public string? Notes { get; set; } // Same as InvoiceNote
    public bool IsInvoiceNotePrintable { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public DateTime TimeStamp { get; set; }
    public DateTime CreatedAt { get; set; } // Same as TimeStamp
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public virtual Customer? Customer { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
