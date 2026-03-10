using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Services;
using System.Text;

namespace Pos.Web.MigrationUtility;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("MyChair POS - User Migration Utility");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        try
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddCommandLine(args)
                .Build();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            var serviceProvider = services.BuildServiceProvider();

            // Get migration service
            var migrationService = serviceProvider.GetRequiredService<IUserMigrationService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // Parse command line arguments
            var command = args.Length > 0 ? args[0].ToLower() : "migrate-all";
            var forcePasswordReset = !args.Any(a => a.Equals("--no-password-reset", StringComparison.OrdinalIgnoreCase));

            logger.LogInformation("Starting migration utility with command: {Command}", command);

            switch (command)
            {
                case "migrate-all":
                    await MigrateAllUsersAsync(migrationService, forcePasswordReset);
                    break;

                case "migrate-user":
                    if (args.Length < 2 || !int.TryParse(args[1], out int userId))
                    {
                        Console.WriteLine("Error: Please provide a valid user ID");
                        Console.WriteLine("Usage: Pos.Web.MigrationUtility migrate-user <userId>");
                        return 1;
                    }
                    await MigrateSingleUserAsync(migrationService, userId);
                    break;

                case "status":
                    await GetMigrationStatusAsync(migrationService);
                    break;

                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Use 'help' to see available commands");
                    return 1;
            }

            logger.LogInformation("Migration utility completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("FATAL ERROR:");
            Console.WriteLine(ex.Message);
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            return 1;
        }
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add database contexts
        var posConnectionString = configuration.GetConnectionString("PosDatabase");
        var membershipConnectionString = configuration.GetConnectionString("WebPosMembership");

        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlServer(posConnectionString));

        services.AddDbContext<WebPosMembershipDbContext>(options =>
            options.UseSqlServer(membershipConnectionString));

        // Add Identity services manually (console app doesn't have AddIdentity extension)
        services.AddScoped<UserManager<ApplicationUser>>();
        services.AddScoped<RoleManager<ApplicationRole>>();
        services.AddScoped<IUserStore<ApplicationUser>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<ApplicationUser, ApplicationRole, WebPosMembershipDbContext>>();
        services.AddScoped<IRoleStore<ApplicationRole>, Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<ApplicationRole, WebPosMembershipDbContext>>();
        services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddScoped<IdentityErrorDescriber>();
        
        // Configure Identity options
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = false;
        });

        // Add services
        services.AddScoped<IAuditLoggingService, AuditLoggingService>();
        services.AddScoped<IUserMigrationService, UserMigrationService>();
    }

    static async Task MigrateAllUsersAsync(IUserMigrationService migrationService, bool forcePasswordReset)
    {
        Console.WriteLine("Starting migration of all active users...");
        Console.WriteLine($"Force password reset: {(forcePasswordReset ? "Yes" : "No")}");
        Console.WriteLine();

        var result = await migrationService.MigrateAllUsersAsync(forcePasswordReset);

        Console.WriteLine();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("MIGRATION RESULTS");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
        Console.WriteLine($"Total Users: {result.TotalUsers}");
        Console.WriteLine($"Successful: {result.SuccessfulMigrations}");
        Console.WriteLine($"Failed: {result.FailedMigrations}");
        Console.WriteLine($"Skipped: {result.SkippedUsers}");
        Console.WriteLine();

        if (result.Errors.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ERRORS:");
            Console.WriteLine("-".PadRight(80, '-'));
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"User ID {error.LegacyUserId} ({error.UserName}): {error.ErrorMessage}");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        if (result.MigratedUsers.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESSFULLY MIGRATED USERS:");
            Console.WriteLine("-".PadRight(80, '-'));
            Console.ResetColor();

            // Save temporary passwords to file
            var passwordsFile = $"migrated-users-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            using (var writer = new StreamWriter(passwordsFile))
            {
                writer.WriteLine("MyChair POS - Migrated Users Temporary Passwords");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine("=".PadRight(80, '='));
                writer.WriteLine();
                writer.WriteLine("IMPORTANT: Distribute these passwords securely to users.");
                writer.WriteLine("Users will be required to change their password on first login.");
                writer.WriteLine();
                writer.WriteLine("-".PadRight(80, '-'));
                writer.WriteLine();

                foreach (var user in result.MigratedUsers)
                {
                    Console.WriteLine($"✓ {user.DisplayName} ({user.UserName}) - Role: {user.Role}");
                    writer.WriteLine($"User: {user.DisplayName}");
                    writer.WriteLine($"Username: {user.UserName}");
                    writer.WriteLine($"Role: {user.Role}");
                    writer.WriteLine($"Temporary Password: {user.TemporaryPassword}");
                    writer.WriteLine($"Legacy User ID: {user.LegacyUserId}");
                    writer.WriteLine($"Identity User ID: {user.IdentityUserId}");
                    writer.WriteLine();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Temporary passwords saved to: {passwordsFile}");
            Console.WriteLine("IMPORTANT: Distribute these passwords securely and delete the file after distribution.");
            Console.ResetColor();
        }

        Console.WriteLine();
        if (result.IsSuccessful)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Migration completed successfully!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ Migration completed with errors. Please review the error log above.");
        }
        Console.ResetColor();
    }

    static async Task MigrateSingleUserAsync(IUserMigrationService migrationService, int userId)
    {
        Console.WriteLine($"Starting migration of user with ID {userId}...");
        Console.WriteLine();

        var result = await migrationService.MigrateSingleUserAsync(userId);

        Console.WriteLine();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("MIGRATION RESULT");
        Console.WriteLine("=".PadRight(80, '='));

        if (result.SuccessfulMigrations > 0)
        {
            var user = result.MigratedUsers.First();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ User migrated successfully!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Display Name: {user.DisplayName}");
            Console.WriteLine($"Username: {user.UserName}");
            Console.WriteLine($"Role: {user.Role}");
            Console.WriteLine($"Legacy User ID: {user.LegacyUserId}");
            Console.WriteLine($"Identity User ID: {user.IdentityUserId}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Temporary Password: {user.TemporaryPassword}");
            Console.WriteLine("IMPORTANT: Distribute this password securely to the user.");
            Console.WriteLine("User will be required to change password on first login.");
            Console.ResetColor();
        }
        else if (result.SkippedUsers > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ User already migrated. No action taken.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Migration failed!");
            Console.ResetColor();
            if (result.Errors.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Error:");
                Console.WriteLine(result.Errors.First().ErrorMessage);
            }
        }
    }

    static async Task GetMigrationStatusAsync(IUserMigrationService migrationService)
    {
        Console.WriteLine("Retrieving migration status...");
        Console.WriteLine();

        var report = await migrationService.GetMigrationStatusAsync();

        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("MIGRATION STATUS");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine($"Total Legacy Users: {report.TotalLegacyUsers}");
        Console.WriteLine($"Migrated Users: {report.MigratedUsersCount}");
        Console.WriteLine($"Pending Migration: {report.PendingMigrationCount}");
        Console.WriteLine($"Progress: {report.MigrationPercentage:F1}%");
        
        if (report.LastMigrationDate.HasValue)
        {
            Console.WriteLine($"Last Migration: {report.LastMigrationDate.Value:yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            Console.WriteLine("Last Migration: Never");
        }

        Console.WriteLine();
        if (report.IsComplete)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ All users have been migrated!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {report.PendingMigrationCount} user(s) pending migration.");
        }
        Console.ResetColor();
    }

    static void ShowHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  Pos.Web.MigrationUtility [command] [options]");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  migrate-all              Migrate all active users from legacy database");
        Console.WriteLine("                           Options: --no-password-reset (skip password reset requirement)");
        Console.WriteLine();
        Console.WriteLine("  migrate-user <userId>    Migrate a single user by legacy user ID");
        Console.WriteLine();
        Console.WriteLine("  status                   Show migration status and statistics");
        Console.WriteLine();
        Console.WriteLine("  help                     Show this help message");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  Pos.Web.MigrationUtility migrate-all");
        Console.WriteLine("  Pos.Web.MigrationUtility migrate-all --no-password-reset");
        Console.WriteLine("  Pos.Web.MigrationUtility migrate-user 123");
        Console.WriteLine("  Pos.Web.MigrationUtility status");
        Console.WriteLine();
        Console.WriteLine("CONFIGURATION:");
        Console.WriteLine("  Connection strings are read from appsettings.json");
        Console.WriteLine("  Required connection strings:");
        Console.WriteLine("    - PosDatabase: Legacy POS database");
        Console.WriteLine("    - WebPosMembership: New membership database");
    }
}
