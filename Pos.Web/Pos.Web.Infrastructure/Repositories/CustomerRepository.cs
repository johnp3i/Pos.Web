using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Customer repository implementation with customer-specific methods
/// Following JDS repository design guidelines with async/await and try/catch patterns
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(PosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Search customers by name or phone (fuzzy search)
    /// </summary>
    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            var normalizedSearch = searchTerm.Trim().ToLower();
            
            return await _dbSet
                .Include(c => c.Addresses)
                .Where(c => c.IsActive &&
                    (c.Name.ToLower().Contains(normalizedSearch) ||
                     c.Telephone.Contains(normalizedSearch) ||
                     (c.Email != null && c.Email.ToLower().Contains(normalizedSearch))))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get customer with addresses
    /// </summary>
    public async Task<Customer?> GetCustomerWithAddressesAsync(int customerId)
    {
        try
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.ID == customerId);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get customer by phone number
    /// </summary>
    public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var normalizedPhone = phone.Trim();
            
            return await _dbSet
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Telephone == normalizedPhone);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Check if customer with same name and phone already exists
    /// </summary>
    public async Task<bool> CheckDuplicateCustomerAsync(string name, string phone, int? excludeCustomerId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            var normalizedName = name.Trim().ToLower();
            var normalizedPhone = phone.Trim();
            
            var query = _dbSet.Where(c => 
                c.Name.ToLower() == normalizedName && 
                c.Telephone == normalizedPhone);
            
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.ID != excludeCustomerId.Value);
            }
            
            return await query.AnyAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get customers with loyalty points above threshold
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomersWithLoyaltyPointsAsync(int minPoints)
    {
        try
        {
            return await _dbSet
                .Where(c => c.IsActive && c.LoyaltyPoints >= minPoints)
                .OrderByDescending(c => c.LoyaltyPoints)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get recently active customers (ordered in last N days)
    /// </summary>
    public async Task<IEnumerable<Customer>> GetRecentlyActiveCustomersAsync(int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            
            return await _dbSet
                .Include(c => c.Orders)
                .Where(c => c.IsActive && 
                    c.Orders.Any(o => o.TimeStamp >= cutoffDate))
                .OrderByDescending(c => c.Orders.Max(o => o.TimeStamp))
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get top customers by total spent
    /// </summary>
    public async Task<IEnumerable<Customer>> GetTopCustomersBySpentAsync(int topCount = 10)
    {
        try
        {
            return await _dbSet
                .Include(c => c.Orders)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.Orders.Sum(o => o.TotalCost))
                .Take(topCount)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
