using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for payment request
/// </summary>
public class PaymentRequestDto
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
    /// Payment amount
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Amount tendered by customer (for cash payments)
    /// </summary>
    public decimal? AmountTendered { get; set; }
    
    /// <summary>
    /// Payment reference number (for card/bank payments)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// User ID processing the payment
    /// </summary>
    [Required]
    public int ProcessedBy { get; set; }
    
    /// <summary>
    /// Optional notes about the payment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
