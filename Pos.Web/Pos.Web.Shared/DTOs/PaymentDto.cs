using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a payment
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Payment ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Order ID
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
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment reference number (for card/bank payments)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Payment timestamp
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// User ID who processed the payment
    /// </summary>
    [Required]
    public int ProcessedBy { get; set; }
}
