using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a customer
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Customer ID (0 for new customers)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Customer name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer telephone number
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Phone]
    public string Telephone { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer email address
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Customer addresses
    /// </summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();
    
    /// <summary>
    /// Loyalty points balance
    /// </summary>
    public int LoyaltyPoints { get; set; }
    
    /// <summary>
    /// Total number of orders
    /// </summary>
    public int TotalOrders { get; set; }
    
    /// <summary>
    /// Total amount spent
    /// </summary>
    public decimal TotalSpent { get; set; }
    
    /// <summary>
    /// Last order date
    /// </summary>
    public DateTime? LastOrderDate { get; set; }
    
    /// <summary>
    /// Customer creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Whether customer is active
    /// </summary>
    public bool IsActive { get; set; }
}
