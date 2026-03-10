using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for an order
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Order ID (0 for new orders)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Customer ID (null for walk-in customers)
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Customer information
    /// </summary>
    public CustomerDto? Customer { get; set; }
    
    /// <summary>
    /// User ID who created the order
    /// </summary>
    [Required]
    public int UserId { get; set; }
    
    /// <summary>
    /// Service type (Dine-in, Takeout, Delivery)
    /// </summary>
    [Required]
    public ServiceType ServiceType { get; set; }
    
    /// <summary>
    /// Table number (for dine-in orders)
    /// </summary>
    public byte? TableNumber { get; set; }
    
    /// <summary>
    /// Order status
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Order items
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<OrderItemDto> Items { get; set; } = new();
    
    /// <summary>
    /// Subtotal (sum of item prices before tax and discounts)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Tax amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }
    
    /// <summary>
    /// Discount amount (fixed amount)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? DiscountAmount { get; set; }
    
    /// <summary>
    /// Voucher ID (if voucher applied)
    /// </summary>
    public int? VoucherId { get; set; }
    
    /// <summary>
    /// Total amount (subtotal + tax - discounts)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount paid by customer
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? AmountPaid { get; set; }
    
    /// <summary>
    /// Change to return to customer
    /// </summary>
    public decimal? ChangeAmount { get; set; }
    
    /// <summary>
    /// Order notes (special instructions)
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether notes should be printed on receipt
    /// </summary>
    public bool IsNotesPrintable { get; set; }
    
    /// <summary>
    /// Scheduled time for order (for future orders)
    /// </summary>
    public DateTime? ScheduledTime { get; set; }
    
    /// <summary>
    /// Order creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Order last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Order completion timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
