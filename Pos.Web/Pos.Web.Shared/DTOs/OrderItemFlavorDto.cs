using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for order item flavors/variations
/// </summary>
public class OrderItemFlavorDto
{
    /// <summary>
    /// Flavor ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Flavor name (e.g., "Vanilla", "Caramel", "Hazelnut")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional price for this flavor (usually 0)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
