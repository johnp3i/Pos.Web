namespace Pos.Web.Shared.DTOs;

/// <summary>
/// Data transfer object for payment processing result
/// </summary>
public class PaymentResultDto
{
    /// <summary>
    /// Whether the payment was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Payment transaction ID
    /// </summary>
    public int PaymentId { get; set; }
    
    /// <summary>
    /// Transaction reference number
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Change amount (for cash payments)
    /// </summary>
    public decimal? ChangeAmount { get; set; }
    
    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Payment timestamp
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// Receipt number
    /// </summary>
    public string? ReceiptNumber { get; set; }
}
