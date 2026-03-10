namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Category entity (placeholder for legacy dbo.Categories table)
/// This will be mapped to the actual legacy table structure later
/// </summary>
public class Category
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    //public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
