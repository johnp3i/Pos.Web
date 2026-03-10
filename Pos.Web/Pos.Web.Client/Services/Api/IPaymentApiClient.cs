using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for payment operations
/// </summary>
public interface IPaymentApiClient
{
    /// <summary>
    /// Process a payment for an order
    /// </summary>
    Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
    
    /// <summary>
    /// Get payment methods
    /// </summary>
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync();
    
    /// <summary>
    /// Validate payment before processing
    /// </summary>
    Task<PaymentValidationResultDto> ValidatePaymentAsync(PaymentRequestDto paymentRequest);
    
    /// <summary>
    /// Get payment history for an order
    /// </summary>
    Task<List<PaymentDto>> GetOrderPaymentsAsync(int orderId);
}
