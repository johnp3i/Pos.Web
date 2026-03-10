namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for payment validation result
/// </summary>
public class PaymentValidationResultDto
{
    /// <summary>
    /// Whether the payment request is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Order total amount
    /// </summary>
    public decimal OrderTotal { get; set; }
    
    /// <summary>
    /// Amount already paid
    /// </summary>
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Remaining balance to be paid
    /// </summary>
    public decimal RemainingBalance { get; set; }
    
    /// <summary>
    /// Whether the order can accept partial payments
    /// </summary>
    public bool AllowsPartialPayment { get; set; }
}
