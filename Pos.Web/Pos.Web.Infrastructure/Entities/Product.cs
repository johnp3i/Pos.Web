namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Product entity (mapped to legacy dbo.CategoryItems table)
/// Note: Some properties don't exist in legacy table and are provided with default values
/// </summary>
public class Product
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public int DisplayOrder { get; set; }
    
    // Properties that don't exist in legacy table - provide default values
    public string? Description { get; set; } = null;
    public decimal Price { get; set; } = 0m;
    public string? ImageUrl { get; set; } = null;
    public string? Barcode { get; set; } = null;
    public bool IsAvailable { get; set; } = true;
    public bool IsInStock { get; set; } = true;
    public int StockQuantity { get; set; } = 999;
    public bool IsFavorite { get; set; } = false;
    
    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
