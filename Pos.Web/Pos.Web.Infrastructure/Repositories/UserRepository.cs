using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User operations
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly PosDbContext _context;

    public UserRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Name == username && u.IsActive);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ID == id && u.IsActive);
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.DisplayOrder)
            .ToListAsync();
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        // Note: Legacy system stores passwords in plain text
        // This should be migrated to hashed passwords in production
        return await _context.Users
            .FirstOrDefaultAsync(u => 
                u.Name == username && 
                u.Password == password && 
                u.IsActive);
    }
}
