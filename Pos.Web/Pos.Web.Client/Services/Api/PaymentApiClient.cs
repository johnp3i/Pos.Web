using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for payment operations
/// </summary>
public class PaymentApiClient : IPaymentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentApiClient> _logger;
    
    public PaymentApiClient(HttpClient httpClient, ILogger<PaymentApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}, Amount: {Amount}", 
                paymentRequest.OrderId, paymentRequest.Amount);
            
            var response = await _httpClient.PostAsJsonAsync("/api/payments/process", paymentRequest);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<PaymentResultDto>();
            
            if (result == null)
            {
                _logger.LogError("Payment processing returned null result for order {OrderId}", paymentRequest.OrderId);
                throw new Exception("Payment processing failed - no result returned");
            }
            
            _logger.LogInformation("Payment processed successfully for order {OrderId}, Transaction: {TransactionId}", 
                paymentRequest.OrderId, result.TransactionId);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error processing payment for order {OrderId}", paymentRequest.OrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", paymentRequest.OrderId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync()
    {
        try
        {
            _logger.LogDebug("Loading payment methods");
            
            var response = await _httpClient.GetFromJsonAsync<List<PaymentMethodDto>>("/api/payments/methods");
            
            if (response == null)
            {
                _logger.LogWarning("Payment methods API returned null, returning empty list");
                return new List<PaymentMethodDto>();
            }
            
            _logger.LogDebug("Loaded {Count} payment methods", response.Count);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading payment methods");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment methods");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<PaymentValidationResultDto> ValidatePaymentAsync(PaymentRequestDto paymentRequest)
    {
        try
        {
            _logger.LogDebug("Validating payment for order {OrderId}", paymentRequest.OrderId);
            
            var response = await _httpClient.PostAsJsonAsync("/api/payments/validate", paymentRequest);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<PaymentValidationResultDto>();
            
            if (result == null)
            {
                _logger.LogError("Payment validation returned null result for order {OrderId}", paymentRequest.OrderId);
                throw new Exception("Payment validation failed - no result returned");
            }
            
            _logger.LogDebug("Payment validation completed for order {OrderId}, IsValid: {IsValid}", 
                paymentRequest.OrderId, result.IsValid);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error validating payment for order {OrderId}", paymentRequest.OrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment for order {OrderId}", paymentRequest.OrderId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<PaymentDto>> GetOrderPaymentsAsync(int orderId)
    {
        try
        {
            _logger.LogDebug("Loading payment history for order {OrderId}", orderId);
            
            var response = await _httpClient.GetFromJsonAsync<List<PaymentDto>>($"/api/payments/order/{orderId}");
            
            if (response == null)
            {
                _logger.LogWarning("Payment history API returned null for order {OrderId}, returning empty list", orderId);
                return new List<PaymentDto>();
            }
            
            _logger.LogDebug("Loaded {Count} payments for order {OrderId}", response.Count, orderId);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading payment history for order {OrderId}", orderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment history for order {OrderId}", orderId);
            throw;
        }
    }
}
