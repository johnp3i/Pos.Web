using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for applying a discount to an order
/// </summary>
public class ApplyDiscountRequest
{
    /// <summary>
    /// Order ID to apply discount to
    /// </summary>
    [Required]
    public int OrderId { get; set; }
    
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
    /// Discount reason
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Manager ID who approved (if required)
    /// </summary>
    public int? ApprovedBy { get; set; }
}
