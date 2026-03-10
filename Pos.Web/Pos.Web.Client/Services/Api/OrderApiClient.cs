using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for order operations
/// </summary>
public class OrderApiClient : IOrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderApiClient> _logger;
    
    public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<List<PendingOrderDto>> GetPendingOrdersAsync()
    {
        try
        {
            _logger.LogDebug("Loading pending orders");
            
            var response = await _httpClient.GetFromJsonAsync<List<PendingOrderDto>>("/api/orders/pending");
            
            if (response == null)
            {
                _logger.LogWarning("Pending orders API returned null, returning empty list");
                return new List<PendingOrderDto>();
            }
            
            _logger.LogDebug("Loaded {Count} pending orders", response.Count);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading pending orders");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending orders");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<OrderDto> GetPendingOrderAsync(int pendingOrderId)
    {
        try
        {
            _logger.LogDebug("Loading pending order {PendingOrderId}", pendingOrderId);
            
            var response = await _httpClient.GetFromJsonAsync<OrderDto>($"/api/orders/pending/{pendingOrderId}");
            
            if (response == null)
            {
                _logger.LogWarning("Pending order {PendingOrderId} not found", pendingOrderId);
                throw new Exception($"Pending order {pendingOrderId} not found");
            }
            
            _logger.LogDebug("Loaded pending order {PendingOrderId}", pendingOrderId);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading pending order {PendingOrderId}", pendingOrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending order {PendingOrderId}", pendingOrderId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<int> SaveAsPendingAsync(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Saving order as pending");
            
            var response = await _httpClient.PostAsJsonAsync("/api/orders/pending", order);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<int>();
            
            _logger.LogInformation("Saved order as pending with ID {PendingOrderId}", result);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error saving order as pending");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order as pending");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task DeletePendingOrderAsync(int pendingOrderId)
    {
        try
        {
            _logger.LogInformation("Deleting pending order {PendingOrderId}", pendingOrderId);
            
            var response = await _httpClient.DeleteAsync($"/api/orders/pending/{pendingOrderId}");
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Deleted pending order {PendingOrderId}", pendingOrderId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error deleting pending order {PendingOrderId}", pendingOrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pending order {PendingOrderId}", pendingOrderId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<int> CreateOrderAsync(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", order.CustomerId);
            
            var response = await _httpClient.PostAsJsonAsync("/api/orders", order);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<int>();
            
            _logger.LogInformation("Created order {OrderId} for customer {CustomerId}", result, order.CustomerId);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating order for customer {CustomerId}", order.CustomerId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", order.CustomerId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task UpdateOrderAsync(int orderId, OrderDto order)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId}", orderId);
            
            var response = await _httpClient.PutAsJsonAsync($"/api/orders/{orderId}", order);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Updated order {OrderId}", orderId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error updating order {OrderId}", orderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", orderId);
            throw;
        }
    }
}
