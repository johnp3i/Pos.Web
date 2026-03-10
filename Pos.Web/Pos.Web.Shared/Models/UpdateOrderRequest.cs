using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for updating an existing order
/// </summary>
public class UpdateOrderRequest
{
    /// <summary>
    /// Order ID to update
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    
    /// <summary>
    /// Customer ID (null for walk-in customers)
    /// </summary>
    public int? CustomerId { get; set; }
    
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
    /// Order notes (special instructions)
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether notes should be printed on receipt
    /// </summary>
    public bool IsNotesPrintable { get; set; }
}
