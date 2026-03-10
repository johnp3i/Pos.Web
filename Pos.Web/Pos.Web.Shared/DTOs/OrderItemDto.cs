using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for an order item
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Order item ID (0 for new items)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product/Category item ID
    /// </summary>
    [Required]
    public int ProductId { get; set; }
    
    /// <summary>
    /// Product name (for display purposes)
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Product information
    /// </summary>
    public ProductDto? Product { get; set; }
    
    /// <summary>
    /// Quantity ordered
    /// </summary>
    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }
    
    /// <summary>
    /// Unit price at time of order
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Total price (quantity * unit price)
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Item-specific notes (e.g., "No ice", "Extra hot")
    /// </summary>
    [MaxLength(200)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Extra items/modifiers (e.g., extra shot, whipped cream)
    /// </summary>
    public List<OrderItemExtraDto> Extras { get; set; } = new();
    
    /// <summary>
    /// Flavors/variations (e.g., vanilla, caramel)
    /// </summary>
    public List<OrderItemFlavorDto> Flavors { get; set; } = new();
}
