using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Data;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Health check controller for monitoring application status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly PosDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(PosDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Application = "MyChair POS API",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Database connectivity check
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            // Try to connect to database
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                return Ok(new
                {
                    Status = "Healthy",
                    Database = "Connected",
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Database = "Cannot connect",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Database = "Error",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
