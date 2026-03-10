namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Customer entity (placeholder for legacy dbo.Customers table)
/// This will be mapped to the actual legacy table structure later
/// </summary>
public class Customer
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int LoyaltyPoints { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
