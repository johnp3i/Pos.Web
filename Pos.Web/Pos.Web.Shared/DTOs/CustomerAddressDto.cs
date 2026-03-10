using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for a customer address
/// </summary>
public class CustomerAddressDto
{
    /// <summary>
    /// Address ID (0 for new addresses)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Address line 1 (street address)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;
    
    /// <summary>
    /// Address line 2 (apartment, suite, etc.)
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Whether this is the default address
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Delivery instructions
    /// </summary>
    [MaxLength(500)]
    public string? DeliveryInstructions { get; set; }
}
