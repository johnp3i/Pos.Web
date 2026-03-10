using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for payment method configuration
/// </summary>
public class PaymentMethodDto
{
    /// <summary>
    /// Payment method type
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Display name for the payment method
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this payment method is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Icon name for UI display
    /// </summary>
    public string? IconName { get; set; }
    
    /// <summary>
    /// Whether this payment method requires a reference number
    /// </summary>
    public bool RequiresReference { get; set; }
    
    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; set; }
}
