namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Customer address entity (placeholder for legacy dbo.CustomerAddresses table)
/// This will be mapped to the actual legacy table structure later
/// </summary>
public class CustomerAddress
{
    public int ID { get; set; }
    public int CustomerID { get; set; }
    public string Address { get; set; } = string.Empty; // Legacy field
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public string? DeliveryInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer? Customer { get; set; }
}
