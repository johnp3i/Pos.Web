using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Data;

/// <summary>
/// Database initializer for seeding initial data into WebPosMembership database.
/// Ensures system roles are created on application startup.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial roles if they don't exist.
    /// This method is idempotent and safe to call multiple times.
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<WebPosMembershipDbContext>>();

        logger.LogInformation("Starting role seeding process...");

        var roles = new[]
        {
            new { Name = ApplicationRole.Admin, Description = "System administrator with full access to all features" },
            new { Name = ApplicationRole.Manager, Description = "Store manager with elevated privileges for reporting and configuration" },
            new { Name = ApplicationRole.Cashier, Description = "Cashier with POS access for processing sales and payments" },
            new { Name = ApplicationRole.Waiter, Description = "Waiter with order management access for table service" },
            new { Name = ApplicationRole.Kitchen, Description = "Kitchen staff with order preparation and fulfillment access" }
        };

        foreach (var roleInfo in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleInfo.Name);
            
            if (!roleExists)
            {
                var role = new ApplicationRole
                {
                    Name = roleInfo.Name,
                    Description = roleInfo.Description,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' created successfully", roleInfo.Name);
                }
                else
                {
                    logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                        roleInfo.Name, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Role '{RoleName}' already exists, skipping", roleInfo.Name);
            }
        }

        logger.LogInformation("Role seeding process completed");
    }

    /// <summary>
    /// Initializes the database by ensuring it's created and seeded with initial data.
    /// Call this method during application startup.
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebPosMembershipDbContext>>();

        try
        {
            logger.LogInformation("Initializing WebPosMembership database...");

            var context = services.GetRequiredService<WebPosMembershipDbContext>();
            
            // Ensure database is created (will not recreate if exists)
            await context.Database.EnsureCreatedAsync();
            
            logger.LogInformation("Database ensured to exist");

            // Seed roles
            await SeedRolesAsync(services);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
