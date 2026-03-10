using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Client.Services.Api;

/// <summary>
/// API client for customer operations
/// </summary>
public class CustomerApiClient : ICustomerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiClient> _logger;
    
    public CustomerApiClient(HttpClient httpClient, ILogger<CustomerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<List<CustomerDto>> SearchCustomersAsync(string query)
    {
        try
        {
            _logger.LogDebug("Searching customers with query: {Query}", query);
            
            var response = await _httpClient.GetFromJsonAsync<List<CustomerDto>>($"/api/customers/search?q={Uri.EscapeDataString(query)}");
            
            if (response == null)
            {
                _logger.LogWarning("Customer search returned null for query: {Query}, returning empty list", query);
                return new List<CustomerDto>();
            }
            
            _logger.LogDebug("Found {Count} customers for query: {Query}", response.Count, query);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error searching customers with query: {Query}", query);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with query: {Query}", query);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<CustomerDto> GetCustomerAsync(int customerId)
    {
        try
        {
            _logger.LogDebug("Loading customer {CustomerId}", customerId);
            
            var response = await _httpClient.GetFromJsonAsync<CustomerDto>($"/api/customers/{customerId}");
            
            if (response == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                throw new Exception($"Customer {customerId} not found");
            }
            
            _logger.LogDebug("Loaded customer {CustomerId}: {CustomerName}", customerId, response.Name);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading customer {CustomerId}", customerId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer {CustomerId}", customerId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
    {
        try
        {
            _logger.LogInformation("Creating customer: {CustomerName}", customer.Name);
            
            var response = await _httpClient.PostAsJsonAsync("/api/customers", customer);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<CustomerDto>();
            
            if (result == null)
            {
                _logger.LogError("Customer creation returned null result for: {CustomerName}", customer.Name);
                throw new Exception("Failed to create customer - no result returned");
            }
            
            _logger.LogInformation("Created customer {CustomerId}: {CustomerName}", result.Id, result.Name);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating customer: {CustomerName}", customer.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {CustomerName}", customer.Name);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<OrderDto>> GetCustomerHistoryAsync(int customerId)
    {
        try
        {
            _logger.LogDebug("Loading order history for customer {CustomerId}", customerId);
            
            var response = await _httpClient.GetFromJsonAsync<List<OrderDto>>($"/api/customers/{customerId}/history");
            
            if (response == null)
            {
                _logger.LogWarning("Customer history returned null for customer {CustomerId}, returning empty list", customerId);
                return new List<OrderDto>();
            }
            
            _logger.LogDebug("Loaded {Count} orders for customer {CustomerId}", response.Count, customerId);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading history for customer {CustomerId}", customerId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading history for customer {CustomerId}", customerId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<CustomerDto>> GetRecentCustomersAsync()
    {
        try
        {
            _logger.LogDebug("Loading recent customers");
            
            var response = await _httpClient.GetFromJsonAsync<List<CustomerDto>>("/api/customers/recent");
            
            if (response == null)
            {
                _logger.LogWarning("Recent customers API returned null, returning empty list");
                return new List<CustomerDto>();
            }
            
            _logger.LogDebug("Loaded {Count} recent customers", response.Count);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error loading recent customers");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent customers");
            throw;
        }
    }
}
