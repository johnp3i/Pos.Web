using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for processing split payment (multiple payment methods)
/// </summary>
public class SplitPaymentRequest
{
    /// <summary>
    /// Order ID to process payment for
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    
    /// <summary>
    /// List of payment methods and amounts
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "Split payment requires at least 2 payment methods")]
    public List<SplitPaymentItem> Payments { get; set; } = new();
    
    /// <summary>
    /// Whether to print receipt
    /// </summary>
    public bool PrintReceipt { get; set; } = true;
    
    /// <summary>
    /// Whether to open cash drawer
    /// </summary>
    public bool OpenCashDrawer { get; set; } = true;
}

/// <summary>
/// Individual payment item in a split payment
/// </summary>
public class SplitPaymentItem
{
    /// <summary>
    /// Payment method
    /// </summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Amount for this payment method
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment reference number (for card/bank payments)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
}
