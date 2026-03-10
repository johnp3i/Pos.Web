using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.UnitOfWork;
using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Customer service implementation for managing customer operations
/// Implements search, creation, duplicate detection, and history retrieval
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IApiAuditLogService _auditLogService;
    private readonly ILogger<CustomerService> _logger;

    // Cache key constants
    private const string CacheKeyPrefix = "customers";
    private const string SearchCacheKeyPrefix = $"{CacheKeyPrefix}:search";
    private const string RecentCacheKey = $"{CacheKeyPrefix}:recent";
    private const string TopCacheKey = $"{CacheKeyPrefix}:top";
    
    // Cache expiration times
    private static readonly TimeSpan SearchCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CustomerCacheExpiration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ListCacheExpiration = TimeSpan.FromMinutes(10);

    public CustomerService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IApiAuditLogService auditLogService,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<CustomerDto>> SearchCustomersAsync(string searchTerm, int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<CustomerDto>();
            }

            var cacheKey = $"{SearchCacheKeyPrefix}:{searchTerm}:{limit}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Searching customers with term '{SearchTerm}' (limit: {Limit})", searchTerm, limit);
                
                var customers = await _unitOfWork.Customers.SearchCustomersAsync(searchTerm);
                
                var customerDtos = customers
                    .Where(c => c.IsActive)
                    .Take(limit)
                    .Select(MapToDto)
                    .ToList();
                
                _logger.LogInformation("Found {Count} customers matching '{SearchTerm}'", customerDtos.Count, searchTerm);
                
                return customerDtos;
            }, SearchCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term '{SearchTerm}'", searchTerm);
            throw new ServiceException($"Failed to search customers with term '{searchTerm}'", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
    {
        try
        {
            _logger.LogDebug("Getting customer by ID: {CustomerId}", customerId);
            
            var cacheKey = $"{CacheKeyPrefix}:{customerId}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var customer = await _unitOfWork.Customers.GetCustomerWithAddressesAsync(customerId);
                
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                    return null;
                }
                
                return MapToDto(customer);
            }, CustomerCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", customerId);
            throw new ServiceException($"Failed to get customer {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phone)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            _logger.LogDebug("Getting customer by phone: {Phone}", phone);
            
            var customer = await _unitOfWork.Customers.GetCustomerByPhoneAsync(phone);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found with phone: {Phone}", phone);
                return null;
            }
            
            return MapToDto(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
            throw new ServiceException($"Failed to get customer by phone '{phone}'", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto, int userId)
    {
        try
        {
            _logger.LogInformation("Creating new customer: {Name}, {Phone}", customerDto.Name, customerDto.Telephone);
            
            // Check for duplicate
            var isDuplicate = await _unitOfWork.Customers.CheckDuplicateCustomerAsync(
                customerDto.Name, 
                customerDto.Telephone);
            
            if (isDuplicate)
            {
                var existingCustomer = await _unitOfWork.Customers.GetCustomerByPhoneAsync(customerDto.Telephone);
                _logger.LogWarning("Duplicate customer detected: {Name}, {Phone}", customerDto.Name, customerDto.Telephone);
                throw new DuplicateCustomerException(
                    $"Customer with name '{customerDto.Name}' and phone '{customerDto.Telephone}' already exists",
                    existingCustomer?.ID ?? 0);
            }
            
            // Create customer entity
            var customer = new Entities.Customer
            {
                Name = customerDto.Name,
                Telephone = customerDto.Telephone,
                Email = customerDto.Email,
                LoyaltyPoints = 0,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };
            
            // Add customer
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Customer created successfully: {CustomerId}", customer.ID);
            
            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "Create",
                entityType: "Customer",
                entityId: customer.ID,
                newValues: System.Text.Json.JsonSerializer.Serialize(customer));
            
            // Invalidate cache
            await InvalidateCacheAsync();
            
            return MapToDto(customer);
        }
        catch (DuplicateCustomerException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {Name}", customerDto.Name);
            throw new ServiceException("Failed to create customer", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CustomerDto> UpdateCustomerAsync(int customerId, CustomerDto customerDto, int userId)
    {
        try
        {
            _logger.LogInformation("Updating customer: {CustomerId}", customerId);
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for update: {CustomerId}", customerId);
                throw new ServiceException($"Customer {customerId} not found");
            }
            
            // Check for duplicate (excluding current customer)
            var isDuplicate = await _unitOfWork.Customers.CheckDuplicateCustomerAsync(
                customerDto.Name, 
                customerDto.Telephone,
                customerId);
            
            if (isDuplicate)
            {
                _logger.LogWarning("Duplicate customer detected during update: {Name}, {Phone}", customerDto.Name, customerDto.Telephone);
                throw new DuplicateCustomerException(
                    $"Another customer with name '{customerDto.Name}' and phone '{customerDto.Telephone}' already exists",
                    0);
            }
            
            // Store old values for audit
            var oldValues = System.Text.Json.JsonSerializer.Serialize(customer);
            
            // Update customer
            customer.Name = customerDto.Name;
            customer.Telephone = customerDto.Telephone;
            customer.Email = customerDto.Email;
            customer.IsActive = customerDto.IsActive;
            
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Customer updated successfully: {CustomerId}", customerId);
            
            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "Update",
                entityType: "Customer",
                entityId: customerId,
                oldValues: oldValues,
                newValues: System.Text.Json.JsonSerializer.Serialize(customer));
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:{customerId}");
            await InvalidateCacheAsync();
            
            return MapToDto(customer);
        }
        catch (DuplicateCustomerException)
        {
            throw;
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
            throw new ServiceException($"Failed to update customer {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId, int limit = 50)
    {
        try
        {
            _logger.LogInformation("Getting customer history: {CustomerId} (limit: {Limit})", customerId, limit);
            
            var customer = await _unitOfWork.Customers.GetCustomerWithAddressesAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for history: {CustomerId}", customerId);
                throw new ServiceException($"Customer {customerId} not found");
            }
            
            // Get customer orders
            var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId, limit);
            
            // Calculate statistics
            var allOrders = customer.Orders.ToList();
            var totalOrders = allOrders.Count;
            var totalSpent = allOrders.Sum(o => o.TotalAmount);
            var averageOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;
            var lastOrderDate = allOrders.Any() ? allOrders.Max(o => o.CreatedAt) : (DateTime?)null;
            var firstOrderDate = allOrders.Any() ? allOrders.Min(o => o.CreatedAt) : (DateTime?)null;
            
            var history = new CustomerHistoryDto
            {
                Customer = MapToDto(customer),
                RecentOrders = orders.Select(MapOrderToDto).ToList(),
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                AverageOrderValue = averageOrderValue,
                LastOrderDate = lastOrderDate,
                FirstOrderDate = firstOrderDate
            };
            
            _logger.LogInformation("Customer history retrieved: {CustomerId}, {TotalOrders} orders", customerId, totalOrders);
            
            return history;
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer history: {CustomerId}", customerId);
            throw new ServiceException($"Failed to get customer history for {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task AddLoyaltyPointsAsync(int customerId, int points, int userId, string reason)
    {
        try
        {
            _logger.LogInformation("Adding loyalty points: {CustomerId}, {Points} points, reason: {Reason}", 
                customerId, points, reason);
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for loyalty points: {CustomerId}", customerId);
                throw new ServiceException($"Customer {customerId} not found");
            }
            
            var oldPoints = customer.LoyaltyPoints;
            customer.LoyaltyPoints += points;
            
            // Ensure points don't go negative
            if (customer.LoyaltyPoints < 0)
            {
                customer.LoyaltyPoints = 0;
            }
            
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Loyalty points updated: {CustomerId}, {OldPoints} -> {NewPoints}", 
                customerId, oldPoints, customer.LoyaltyPoints);
            
            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "UpdateLoyaltyPoints",
                entityType: "Customer",
                entityId: customerId,
                oldValues: $"{{\"LoyaltyPoints\":{oldPoints}}}",
                newValues: $"{{\"LoyaltyPoints\":{customer.LoyaltyPoints},\"Reason\":\"{reason}\"}}");
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:{customerId}");
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding loyalty points: {CustomerId}", customerId);
            throw new ServiceException($"Failed to add loyalty points for customer {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<CustomerDto>> GetRecentlyActiveCustomersAsync(int days = 30, int limit = 20)
    {
        try
        {
            var cacheKey = $"{RecentCacheKey}:{days}:{limit}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Getting recently active customers (days: {Days}, limit: {Limit})", days, limit);
                
                var customers = await _unitOfWork.Customers.GetRecentlyActiveCustomersAsync(days);
                
                var customerDtos = customers
                    .Take(limit)
                    .Select(MapToDto)
                    .ToList();
                
                _logger.LogInformation("Found {Count} recently active customers", customerDtos.Count);
                
                return customerDtos;
            }, ListCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently active customers");
            throw new ServiceException("Failed to get recently active customers", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<CustomerDto>> GetTopCustomersAsync(int topCount = 10)
    {
        try
        {
            var cacheKey = $"{TopCacheKey}:{topCount}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                _logger.LogInformation("Getting top customers (count: {TopCount})", topCount);
                
                var customers = await _unitOfWork.Customers.GetTopCustomersBySpentAsync(topCount);
                
                var customerDtos = customers
                    .Select(MapToDto)
                    .ToList();
                
                _logger.LogInformation("Found {Count} top customers", customerDtos.Count);
                
                return customerDtos;
            }, ListCacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top customers");
            throw new ServiceException("Failed to get top customers", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeactivateCustomerAsync(int customerId, int userId)
    {
        try
        {
            _logger.LogInformation("Deactivating customer: {CustomerId}", customerId);
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for deactivation: {CustomerId}", customerId);
                throw new ServiceException($"Customer {customerId} not found");
            }
            
            customer.IsActive = false;
            
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Customer deactivated: {CustomerId}", customerId);
            
            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "Deactivate",
                entityType: "Customer",
                entityId: customerId,
                newValues: "{\"IsActive\":false}");
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:{customerId}");
            await InvalidateCacheAsync();
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating customer: {CustomerId}", customerId);
            throw new ServiceException($"Failed to deactivate customer {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task ReactivateCustomerAsync(int customerId, int userId)
    {
        try
        {
            _logger.LogInformation("Reactivating customer: {CustomerId}", customerId);
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for reactivation: {CustomerId}", customerId);
                throw new ServiceException($"Customer {customerId} not found");
            }
            
            customer.IsActive = true;
            
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Customer reactivated: {CustomerId}", customerId);
            
            // Log audit
            await _auditLogService.LogEntityChangeAsync(
                userId: userId,
                action: "Reactivate",
                entityType: "Customer",
                entityId: customerId,
                newValues: "{\"IsActive\":true}");
            
            // Invalidate cache
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:{customerId}");
            await InvalidateCacheAsync();
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating customer: {CustomerId}", customerId);
            throw new ServiceException($"Failed to reactivate customer {customerId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync()
    {
        try
        {
            _logger.LogInformation("Invalidating customer cache");
            
            await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}:*");
            
            _logger.LogInformation("Customer cache invalidated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating customer cache");
            throw new ServiceException("Failed to invalidate customer cache", ex);
        }
    }

    /// <summary>
    /// Map a Customer entity to a CustomerDto
    /// </summary>
    private static CustomerDto MapToDto(Entities.Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.ID,
            Name = customer.Name,
            Telephone = customer.Telephone,
            Email = customer.Email,
            Addresses = customer.Addresses.Select(MapAddressToDto).ToList(),
            LoyaltyPoints = customer.LoyaltyPoints,
            TotalOrders = customer.Orders.Count,
            TotalSpent = customer.Orders.Sum(o => o.TotalAmount),
            LastOrderDate = customer.Orders.Any() ? customer.Orders.Max(o => o.CreatedAt) : null,
            CreatedAt = customer.RegistrationDate,
            IsActive = customer.IsActive
        };
    }

    /// <summary>
    /// Map a CustomerAddress entity to a CustomerAddressDto
    /// </summary>
    private static CustomerAddressDto MapAddressToDto(Entities.CustomerAddress address)
    {
        return new CustomerAddressDto
        {
            Id = address.ID,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country,
            IsDefault = address.IsDefault,
            DeliveryInstructions = address.DeliveryInstructions
        };
    }

    /// <summary>
    /// Map an Order entity to an OrderDto (simplified for history)
    /// </summary>
    private static OrderDto MapOrderToDto(Entities.Order order)
    {
        // Parse ServiceType from ServiceTypeID
        var serviceType = order.ServiceTypeID switch
        {
            1 => Shared.Enums.ServiceType.DineIn,
            2 => Shared.Enums.ServiceType.Takeout,
            3 => Shared.Enums.ServiceType.Delivery,
            4 => Shared.Enums.ServiceType.DriveThrough,
            _ => Shared.Enums.ServiceType.DineIn
        };
        
        // Parse OrderStatus from Status string
        var status = order.Status?.ToLower() switch
        {
            "draft" => Shared.Enums.OrderStatus.Draft,
            "pending" => Shared.Enums.OrderStatus.Pending,
            "preparing" or "inprogress" => Shared.Enums.OrderStatus.Preparing,
            "ready" => Shared.Enums.OrderStatus.Ready,
            "delivered" => Shared.Enums.OrderStatus.Delivered,
            "completed" => Shared.Enums.OrderStatus.Completed,
            "cancelled" => Shared.Enums.OrderStatus.Cancelled,
            _ => Shared.Enums.OrderStatus.Pending
        };
        
        return new OrderDto
        {
            Id = order.ID,
            CustomerId = order.CustomerID,
            UserId = order.UserID,
            ServiceType = serviceType,
            TableNumber = order.TableNumber,
            Status = status,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountPercentage = order.DiscountPercentage,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            AmountPaid = order.AmountPaid,
            ChangeAmount = order.ChangeAmount,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CompletedAt = order.CompletedAt
        };
    }
}
