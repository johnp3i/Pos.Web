using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a product category
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Category ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category icon/image URL
    /// </summary>
    [MaxLength(500)]
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// Category color (hex code)
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Display order (for sorting)
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether category is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Number of products in this category
    /// </summary>
    public int ProductCount { get; set; }
}
