using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service interface for payment processing operations
/// Handles payment processing, discount application, and split payments
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Process a payment for an order
    /// </summary>
    /// <param name="request">Payment processing request</param>
    /// <param name="userId">User ID processing the payment</param>
    /// <returns>Payment result with order details</returns>
    Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, int userId);
    
    /// <summary>
    /// Apply a discount to an order
    /// </summary>
    /// <param name="request">Discount application request</param>
    /// <param name="userId">User ID applying the discount</param>
    /// <returns>Updated order with discount applied</returns>
    Task<OrderDto> ApplyDiscountAsync(ApplyDiscountRequest request, int userId);
    
    /// <summary>
    /// Process split payment for an order (multiple payment methods)
    /// </summary>
    /// <param name="request">Split payment request</param>
    /// <param name="userId">User ID processing the payment</param>
    /// <returns>Payment result with order details</returns>
    Task<PaymentResult> SplitPaymentAsync(SplitPaymentRequest request, int userId);
    
    /// <summary>
    /// Validate payment amount against order total
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="paymentAmount">Payment amount to validate</param>
    /// <returns>Validation result</returns>
    Task<PaymentValidationResult> ValidatePaymentAsync(int orderId, decimal paymentAmount);
    
    /// <summary>
    /// Calculate change amount for cash payment
    /// </summary>
    /// <param name="orderTotal">Order total amount</param>
    /// <param name="amountPaid">Amount paid by customer</param>
    /// <returns>Change amount</returns>
    decimal CalculateChange(decimal orderTotal, decimal amountPaid);
    
    /// <summary>
    /// Get payment history for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of payments for the order</returns>
    Task<List<PaymentDto>> GetPaymentHistoryAsync(int orderId);
    
    /// <summary>
    /// Refund a payment
    /// </summary>
    /// <param name="paymentId">Payment ID to refund</param>
    /// <param name="userId">User ID processing the refund</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>Refund result</returns>
    Task<RefundResult> RefundPaymentAsync(int paymentId, int userId, string reason);
}

/// <summary>
/// Result of a payment processing operation
/// </summary>
public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderDto? Order { get; set; }
    public decimal ChangeAmount { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of payment validation
/// </summary>
public class PaymentValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal ShortAmount { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of a refund operation
/// </summary>
public class RefundResult
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RefundId { get; set; }
    public decimal RefundAmount { get; set; }
    public List<string> Errors { get; set; } = new();
}
