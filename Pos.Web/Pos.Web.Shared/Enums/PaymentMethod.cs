namespace Pos.Web.Shared.Enums;

/// <summary>
/// Represents the payment method used for an order
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Cash payment
    /// </summary>
    Cash = 1,
    
    /// <summary>
    /// Credit/Debit card payment
    /// </summary>
    Card = 2,
    
    /// <summary>
    /// Voucher/Gift card payment
    /// </summary>
    Voucher = 3,
    
    /// <summary>
    /// Mobile payment (e.g., Apple Pay, Google Pay)
    /// </summary>
    Mobile = 4,
    
    /// <summary>
    /// Bank transfer
    /// </summary>
    BankTransfer = 5,
    
    /// <summary>
    /// Account/Credit (pay later)
    /// </summary>
    Account = 6
}
