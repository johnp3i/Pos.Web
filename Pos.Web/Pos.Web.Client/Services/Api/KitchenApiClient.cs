using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for kitchen operations
/// </summary>
public class KitchenApiClient : IKitchenApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KitchenApiClient> _logger;
    
    public KitchenApiClient(HttpClient httpClient, ILogger<KitchenApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<List<OrderDto>> GetActiveOrdersAsync()
    {
        try
        {
            _logger.LogDebug("Loading active kitchen orders");
            
            var response = await _httpClient.GetFromJsonAsync<List<OrderDto>>("/api/kitchen/orders");
            
            if (response == null)
            {
                _logger.LogWarning("Active kitchen orders API returned null, returning empty list");
                return new List<OrderDto>();
            }
            
            _logger.LogDebug("Loaded {Count} active kitchen orders", response.Count);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading active kitchen orders");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active kitchen orders");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, newStatus);
            
            var response = await _httpClient.PutAsJsonAsync($"/api/kitchen/orders/{orderId}/status", new { Status = newStatus });
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully updated order {OrderId} status to {Status}", orderId, newStatus);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error updating order {OrderId} status to {Status}", orderId, newStatus);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, newStatus);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        try
        {
            _logger.LogDebug("Loading orders with status {Status}", status);
            
            var response = await _httpClient.GetFromJsonAsync<List<OrderDto>>($"/api/kitchen/orders?status={status}");
            
            if (response == null)
            {
                _logger.LogWarning("Orders by status API returned null for status {Status}, returning empty list", status);
                return new List<OrderDto>();
            }
            
            _logger.LogDebug("Loaded {Count} orders with status {Status}", response.Count, status);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading orders with status {Status}", status);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders with status {Status}", status);
            throw;
        }
    }
}
