using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a discount
/// </summary>
public class DiscountDto
{
    /// <summary>
    /// Discount ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Discount name/reason
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Discount percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public decimal? Percentage { get; set; }
    
    /// <summary>
    /// Discount fixed amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Whether discount requires manager approval
    /// </summary>
    public bool RequiresApproval { get; set; }
    
    /// <summary>
    /// Manager ID who approved (if required)
    /// </summary>
    public int? ApprovedBy { get; set; }
    
    /// <summary>
    /// Whether discount is active
    /// </summary>
    public bool IsActive { get; set; }
}
