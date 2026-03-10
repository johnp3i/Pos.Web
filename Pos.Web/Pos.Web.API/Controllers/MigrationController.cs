using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs.Migration;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Controller for user migration operations from legacy dbo.Users table
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MigrationController : ControllerBase
{
    private readonly IUserMigrationService _migrationService;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        IUserMigrationService migrationService,
        ILogger<MigrationController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Migrates all active users from legacy dbo.Users table to WebPosMembership database
    /// </summary>
    /// <param name="forcePasswordReset">If true, all migrated users will be required to change password on first login (default: true)</param>
    /// <returns>Migration result with success/failure counts and error details</returns>
    /// <response code="200">Migration completed successfully</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    /// <response code="500">Internal server error during migration</response>
    [HttpPost("migrate-all")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MigrationResult>> MigrateAllUsers([FromQuery] bool forcePasswordReset = true)
    {
        try
        {
            _logger.LogInformation("Admin user {UserId} initiated migration of all users", User.Identity?.Name);

            var result = await _migrationService.MigrateAllUsersAsync(forcePasswordReset);

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Migration completed successfully: {Summary}", result.Summary);
            }
            else
            {
                _logger.LogWarning("Migration completed with errors: {Summary}", result.Summary);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user migration");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred during migration", message = ex.Message });
        }
    }

    /// <summary>
    /// Migrates a single user from legacy dbo.Users table to WebPosMembership database
    /// </summary>
    /// <param name="legacyUserId">ID of the user in dbo.Users table</param>
    /// <param name="temporaryPassword">Optional temporary password (will be generated if not provided)</param>
    /// <returns>Migration result for the single user</returns>
    /// <response code="200">User migrated successfully</response>
    /// <response code="400">Invalid user ID or user not found</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    /// <response code="500">Internal server error during migration</response>
    [HttpPost("migrate-user/{legacyUserId}")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MigrationResult>> MigrateSingleUser(
        int legacyUserId, 
        [FromQuery] string? temporaryPassword = null)
    {
        try
        {
            if (legacyUserId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID" });
            }

            _logger.LogInformation("Admin user {AdminUser} initiated migration of user with ID {UserId}", 
                User.Identity?.Name, legacyUserId);

            var result = await _migrationService.MigrateSingleUserAsync(legacyUserId, temporaryPassword);

            if (result.SuccessfulMigrations > 0)
            {
                _logger.LogInformation("User {UserId} migrated successfully", legacyUserId);
                return Ok(result);
            }
            else if (result.SkippedUsers > 0)
            {
                _logger.LogInformation("User {UserId} already migrated", legacyUserId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Failed to migrate user {UserId}: {Errors}", 
                    legacyUserId, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during single user migration for user ID {UserId}", legacyUserId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred during migration", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current migration status and statistics
    /// </summary>
    /// <returns>Migration report with statistics</returns>
    /// <response code="200">Migration status retrieved successfully</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(MigrationReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MigrationReport>> GetMigrationStatus()
    {
        try
        {
            var report = await _migrationService.GetMigrationStatusAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while getting migration status", message = ex.Message });
        }
    }

    /// <summary>
    /// Syncs user data from legacy system to WebPosMembership database
    /// Updates DisplayName and other fields from dbo.Users
    /// </summary>
    /// <param name="userId">ID of the user in AspNetUsers table</param>
    /// <returns>Success status</returns>
    /// <response code="200">User data synced successfully</response>
    /// <response code="400">Invalid user ID or user not found</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("sync-user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SyncUserData(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "Invalid user ID" });
            }

            _logger.LogInformation("Admin user {AdminUser} initiated sync for user {UserId}", 
                User.Identity?.Name, userId);

            var success = await _migrationService.SyncUserDataAsync(userId);

            if (success)
            {
                _logger.LogInformation("User data synced successfully for user {UserId}", userId);
                return Ok(new { message = "User data synced successfully" });
            }
            else
            {
                _logger.LogWarning("Failed to sync user data for user {UserId}", userId);
                return BadRequest(new { error = "Failed to sync user data. User may not exist." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user data for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while syncing user data", message = ex.Message });
        }
    }
}
