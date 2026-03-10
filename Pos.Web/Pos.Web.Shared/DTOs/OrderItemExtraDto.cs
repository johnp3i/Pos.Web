using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for order item extras/modifiers
/// </summary>
public class OrderItemExtraDto
{
    /// <summary>
    /// Extra ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Extra name (e.g., "Extra Shot", "Whipped Cream")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional price for this extra
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
