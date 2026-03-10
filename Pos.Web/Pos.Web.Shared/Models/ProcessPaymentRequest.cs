using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for processing a payment
/// </summary>
public class ProcessPaymentRequest
{
    /// <summary>
    /// Order ID to process payment for
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    
    /// <summary>
    /// Payment method
    /// </summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Amount paid by customer
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Payment reference number (for card/bank payments)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Whether to print receipt
    /// </summary>
    public bool PrintReceipt { get; set; } = true;
    
    /// <summary>
    /// Whether to open cash drawer
    /// </summary>
    public bool OpenCashDrawer { get; set; } = true;
}
