using System.ComponentModel.DataAnnotations;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for creating a new customer
/// </summary>
public class CreateCustomerRequest
{
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
    /// Customer address (optional)
    /// </summary>
    public CustomerAddressDto? Address { get; set; }
}
