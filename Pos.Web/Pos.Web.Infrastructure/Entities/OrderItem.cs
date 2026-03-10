namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Order item entity (placeholder for legacy dbo.InvoiceItems table)
/// This will be mapped to the actual legacy table structure later
/// </summary>
public class OrderItem
{
    public int ID { get; set; }
    public int InvoiceID { get; set; }
    public int CategoryItemID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}
