using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a product/category item
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category ID
    /// </summary>
    [Required]
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Category information
    /// </summary>
    public CategoryDto? Category { get; set; }
    
    /// <summary>
    /// Product price
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Product image URL
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Product barcode
    /// </summary>
    [MaxLength(50)]
    public string? Barcode { get; set; }
    
    /// <summary>
    /// Whether product is available
    /// </summary>
    public bool IsAvailable { get; set; }
    
    /// <summary>
    /// Whether product is in stock
    /// </summary>
    public bool IsInStock { get; set; }
    
    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// Display order (for sorting)
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether product is a favorite/featured item
    /// </summary>
    public bool IsFavorite { get; set; }
}
