using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Shared.DTOs.Migration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Service for migrating users from legacy dbo.Users table to WebPosMembership database
/// </summary>
public class UserMigrationService : IUserMigrationService
{
    private readonly PosDbContext _posDbContext;
    private readonly WebPosMembershipDbContext _membershipDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly ILogger<UserMigrationService> _logger;

    public UserMigrationService(
        PosDbContext posDbContext,
        WebPosMembershipDbContext membershipDbContext,
        UserManager<ApplicationUser> userManager,
        IAuditLoggingService auditLoggingService,
        ILogger<UserMigrationService> logger)
    {
        _posDbContext = posDbContext;
        _membershipDbContext = membershipDbContext;
        _userManager = userManager;
        _auditLoggingService = auditLoggingService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<MigrationResult> MigrateAllUsersAsync(bool forcePasswordReset = true)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("Starting migration of all active users from legacy dbo.Users table");

            // Fetch all active users from legacy database
            var legacyUsers = await _posDbContext.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            result.TotalUsers = legacyUsers.Count;
            _logger.LogInformation("Found {Count} active users in legacy database", legacyUsers.Count);

            // Process each user
            foreach (var legacyUser in legacyUsers)
            {
                try
                {
                    // Check if user already migrated
                    var existingUser = await _membershipDbContext.Users
                        .FirstOrDefaultAsync(u => u.EmployeeId == legacyUser.ID);

                    if (existingUser != null)
                    {
                        result.SkippedUsers++;
                        _logger.LogDebug("User {UserName} (ID: {UserId}) already migrated, skipping", 
                            legacyUser.Name, legacyUser.ID);
                        continue;
                    }

                    // Generate temporary password
                    var temporaryPassword = GenerateSecurePassword(12);

                    // Create ApplicationUser
                    var newUser = new ApplicationUser
                    {
                        EmployeeId = legacyUser.ID,
                        UserName = legacyUser.Name,
                        Email = GenerateEmail(legacyUser.Name),
                        FirstName = legacyUser.Name,
                        LastName = legacyUser.Surname ?? string.Empty,
                        DisplayName = legacyUser.FullName,
                        IsActive = legacyUser.IsActive,
                        RequirePasswordChange = forcePasswordReset,
                        CreatedAt = DateTime.UtcNow,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true
                    };

                    // Create user with hashed password
                    var createResult = await _userManager.CreateAsync(newUser, temporaryPassword);

                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        result.FailedMigrations++;
                        result.Errors.Add(new MigrationError
                        {
                            LegacyUserId = legacyUser.ID,
                            UserName = legacyUser.Name,
                            ErrorMessage = $"Failed to create user: {errors}"
                        });
                        _logger.LogWarning("Failed to migrate user {UserName} (ID: {UserId}): {Errors}", 
                            legacyUser.Name, legacyUser.ID, errors);
                        continue;
                    }

                    // Map legacy PositionTypeID to role and assign
                    var roleName = MapPositionTypeToRole(legacyUser.PositionTypeID);
                    var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to assign role {Role} to user {UserName}: {Errors}", 
                            roleName, legacyUser.Name, errors);
                    }

                    // Log successful migration
                    result.SuccessfulMigrations++;
                    result.MigratedUsers.Add(new MigratedUserInfo
                    {
                        LegacyUserId = legacyUser.ID,
                        IdentityUserId = newUser.Id,
                        UserName = newUser.UserName!,
                        DisplayName = newUser.DisplayName!,
                        Role = roleName,
                        TemporaryPassword = temporaryPassword,
                        MigratedAt = DateTime.UtcNow
                    });

                    // Log audit event
                    await _auditLoggingService.LogSecurityEventAsync(
                        Shared.Enums.AuditEventType.UserCreated,
                        newUser.Id,
                        $"User migrated from legacy system. EmployeeId: {legacyUser.ID}, Role: {roleName}");

                    _logger.LogInformation("Successfully migrated user {UserName} (ID: {UserId}) with role {Role}", 
                        legacyUser.Name, legacyUser.ID, roleName);
                }
                catch (Exception ex)
                {
                    result.FailedMigrations++;
                    result.Errors.Add(new MigrationError
                    {
                        LegacyUserId = legacyUser.ID,
                        UserName = legacyUser.Name,
                        ErrorMessage = $"Exception during migration: {ex.Message}"
                    });
                    _logger.LogError(ex, "Exception while migrating user {UserName} (ID: {UserId})", 
                        legacyUser.Name, legacyUser.ID);
                }
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            _logger.LogInformation("Migration completed. {Summary}", result.Summary);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            _logger.LogError(ex, "Fatal error during user migration");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MigrationResult> MigrateSingleUserAsync(int legacyUserId, string? temporaryPassword = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult { TotalUsers = 1 };

        try
        {
            _logger.LogInformation("Starting migration of single user with ID {UserId}", legacyUserId);

            // Fetch user from legacy database
            var legacyUser = await _posDbContext.Users
                .FirstOrDefaultAsync(u => u.ID == legacyUserId && u.IsActive);

            if (legacyUser == null)
            {
                result.FailedMigrations++;
                result.Errors.Add(new MigrationError
                {
                    LegacyUserId = legacyUserId,
                    UserName = "Unknown",
                    ErrorMessage = "User not found in legacy database or is inactive"
                });
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Check if user already migrated
            var existingUser = await _membershipDbContext.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == legacyUserId);

            if (existingUser != null)
            {
                result.SkippedUsers++;
                _logger.LogInformation("User {UserName} (ID: {UserId}) already migrated", 
                    legacyUser.Name, legacyUserId);
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Generate or use provided temporary password
            temporaryPassword ??= GenerateSecurePassword(12);

            // Create ApplicationUser
            var newUser = new ApplicationUser
            {
                EmployeeId = legacyUser.ID,
                UserName = legacyUser.Name,
                Email = GenerateEmail(legacyUser.Name),
                FirstName = legacyUser.Name,
                LastName = legacyUser.Surname ?? string.Empty,
                DisplayName = legacyUser.FullName,
                IsActive = legacyUser.IsActive,
                RequirePasswordChange = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };

            // Create user with hashed password
            var createResult = await _userManager.CreateAsync(newUser, temporaryPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                result.FailedMigrations++;
                result.Errors.Add(new MigrationError
                {
                    LegacyUserId = legacyUser.ID,
                    UserName = legacyUser.Name,
                    ErrorMessage = $"Failed to create user: {errors}"
                });
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Map legacy PositionTypeID to role and assign
            var roleName = MapPositionTypeToRole(legacyUser.PositionTypeID);
            var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to assign role {Role} to user {UserName}: {Errors}", 
                    roleName, legacyUser.Name, errors);
            }

            // Log successful migration
            result.SuccessfulMigrations++;
            result.MigratedUsers.Add(new MigratedUserInfo
            {
                LegacyUserId = legacyUser.ID,
                IdentityUserId = newUser.Id,
                UserName = newUser.UserName!,
                DisplayName = newUser.DisplayName!,
                Role = roleName,
                TemporaryPassword = temporaryPassword,
                MigratedAt = DateTime.UtcNow
            });

            // Log audit event
            await _auditLoggingService.LogSecurityEventAsync(
                Shared.Enums.AuditEventType.UserCreated,
                newUser.Id,
                $"User migrated from legacy system. EmployeeId: {legacyUser.ID}, Role: {roleName}");

            _logger.LogInformation("Successfully migrated user {UserName} (ID: {UserId}) with role {Role}", 
                legacyUser.Name, legacyUser.ID, roleName);

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            return result;
        }
        catch (Exception ex)
        {
            result.FailedMigrations++;
            result.Errors.Add(new MigrationError
            {
                LegacyUserId = legacyUserId,
                UserName = "Unknown",
                ErrorMessage = $"Exception during migration: {ex.Message}"
            });
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            _logger.LogError(ex, "Exception while migrating user with ID {UserId}", legacyUserId);
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<MigrationReport> GetMigrationStatusAsync()
    {
        try
        {
            // Count active users in legacy database
            var totalLegacyUsers = await _posDbContext.Users
                .CountAsync(u => u.IsActive);

            // Count migrated users (users with EmployeeId set)
            var migratedUsersCount = await _membershipDbContext.Users
                .CountAsync(u => u.EmployeeId > 0);

            // Get last migration date from audit logs
            var lastMigrationDate = await _membershipDbContext.AuthAuditLogs
                .Where(a => a.EventType == Shared.Enums.AuditEventType.UserCreated.ToString() 
                         && a.Details != null && a.Details.Contains("migrated from legacy"))
                .OrderByDescending(a => a.Timestamp)
                .Select(a => (DateTime?)a.Timestamp)
                .FirstOrDefaultAsync();

            var report = new MigrationReport
            {
                TotalLegacyUsers = totalLegacyUsers,
                MigratedUsersCount = migratedUsersCount,
                PendingMigrationCount = totalLegacyUsers - migratedUsersCount,
                LastMigrationDate = lastMigrationDate
            };

            _logger.LogInformation("Migration status: {Summary}", report.Summary);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SyncUserDataAsync(string identityUserId)
    {
        try
        {
            _logger.LogInformation("Syncing user data for Identity user {UserId}", identityUserId);

            // Get user from membership database
            var identityUser = await _membershipDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == identityUserId);

            if (identityUser == null)
            {
                _logger.LogWarning("Identity user {UserId} not found", identityUserId);
                return false;
            }

            // Get corresponding legacy user
            var legacyUser = await _posDbContext.Users
                .FirstOrDefaultAsync(u => u.ID == identityUser.EmployeeId);

            if (legacyUser == null)
            {
                _logger.LogWarning("Legacy user with EmployeeId {EmployeeId} not found", identityUser.EmployeeId);
                return false;
            }

            // Update user data from legacy system
            identityUser.FirstName = legacyUser.Name;
            identityUser.LastName = legacyUser.Surname ?? string.Empty;
            identityUser.DisplayName = legacyUser.FullName;
            identityUser.IsActive = legacyUser.IsActive;

            await _membershipDbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully synced user data for {UserName} (ID: {UserId})", 
                identityUser.UserName, identityUserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user data for Identity user {UserId}", identityUserId);
            return false;
        }
    }

    /// <summary>
    /// Maps legacy PositionTypeID to ASP.NET Core Identity role
    /// </summary>
    /// <param name="positionTypeId">Position type ID from legacy system</param>
    /// <returns>Role name</returns>
    private string MapPositionTypeToRole(byte positionTypeId)
    {
        return positionTypeId switch
        {
            1 => ApplicationRole.Cashier,
            2 => ApplicationRole.Admin,
            3 => ApplicationRole.Manager,
            4 => ApplicationRole.Waiter,
            5 => ApplicationRole.Kitchen,
            _ => ApplicationRole.Cashier // Default to Cashier for unknown types
        };
    }

    /// <summary>
    /// Generates a secure random password meeting complexity requirements
    /// </summary>
    /// <param name="length">Length of the password (minimum 12)</param>
    /// <returns>Secure random password</returns>
    private string GenerateSecurePassword(int length = 12)
    {
        if (length < 12)
            length = 12;

        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string nonAlphanumeric = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        const string allChars = lowercase + uppercase + digits + nonAlphanumeric;

        var password = new StringBuilder();

        // Ensure at least one character from each required category
        password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
        password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
        password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
        password.Append(nonAlphanumeric[RandomNumberGenerator.GetInt32(nonAlphanumeric.Length)]);

        // Fill the rest with random characters
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
        }

        // Shuffle the password to avoid predictable patterns
        return new string(password.ToString().OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    /// <summary>
    /// Generates a placeholder email address for users without email
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>Generated email address</returns>
    private string GenerateEmail(string username)
    {
        // Generate a placeholder email using username
        // This can be updated later when actual email addresses are available
        return $"{username.ToLower().Replace(" ", "")}@mychair.local";
    }
}
