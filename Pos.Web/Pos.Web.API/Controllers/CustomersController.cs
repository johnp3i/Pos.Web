using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;
using System.Security.Claims;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Customers controller for managing POS customers
/// Handles customer search, creation, retrieval, and order history
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Search customers by name or phone number
    /// </summary>
    /// <param name="q">Search query (name or phone)</param>
    /// <param name="limit">Maximum number of results (default: 20, max: 100)</param>
    /// <returns>List of matching customers</returns>
    /// <response code="200">Customers found</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchCustomers([FromQuery] string q, [FromQuery] int limit = 20)
    {
        try
        {
            // Validate search query
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiResponse<object>.Error("Search query cannot be empty"));
            }

            if (q.Length < 2)
            {
                return BadRequest(ApiResponse<object>.Error("Search query must be at least 2 characters"));
            }

            // Limit maximum results
            if (limit < 1 || limit > 100)
            {
                limit = 20;
            }

            _logger.LogInformation("Searching customers with query: {Query}, Limit: {Limit}", q, limit);

            var customers = await _customerService.SearchCustomersAsync(q, limit);

            _logger.LogInformation("Found {Count} customers matching query: {Query}", customers.Count, q);
            return Ok(ApiResponse<List<CustomerDto>>.Ok(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchCustomers: Error searching customers with query: {Query}", q);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while searching customers", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer with ID</returns>
    /// <response code="201">Customer created successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="409">Conflict - customer with same name and phone already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("CreateCustomer: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Creating customer: {Name}, Phone: {Phone} by user {UserId}",
                request.Name, request.Telephone, userId.Value);

            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                return BadRequest(ApiResponse<object>.ValidationError(errors));
            }

            // Map request to DTO
            var customerDto = new CustomerDto
            {
                Name = request.Name,
                Telephone = request.Telephone,
                Email = request.Email,
                Addresses = request.Address != null ? new List<CustomerAddressDto> { request.Address } : new()
            };

            // Create customer
            var customer = await _customerService.CreateCustomerAsync(customerDto, userId.Value);

            _logger.LogInformation("Customer created successfully: CustomerId={CustomerId}, Name={Name}",
                customer.Id, customer.Name);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = customer.Id },
                ApiResponse<CustomerDto>.Ok(customer, StatusCodes.Status201Created));
        }
        catch (DuplicateCustomerException ex)
        {
            _logger.LogWarning(ex, "CreateCustomer: Duplicate customer - {Message}", ex.Message);
            return Conflict(ApiResponse<object>.Error(ex.Message, StatusCodes.Status409Conflict));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CreateCustomer: Invalid operation - {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "CreateCustomer: Invalid argument - {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCustomer: Unexpected error creating customer");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while creating the customer", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details with addresses</returns>
    /// <response code="200">Customer found</response>
    /// <response code="404">Customer not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        try
        {
            _logger.LogInformation("Getting customer by ID: {CustomerId}", id);

            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", id);
                return NotFound(ApiResponse<object>.Error($"Customer with ID {id} not found", StatusCodes.Status404NotFound));
            }

            _logger.LogInformation("Customer retrieved successfully: {CustomerId}", id);
            return Ok(ApiResponse<CustomerDto>.Ok(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCustomerById: Error retrieving customer {CustomerId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving the customer", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get customer order history with statistics
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="limit">Maximum number of orders to return (default: 50, max: 200)</param>
    /// <returns>Customer history with orders and statistics</returns>
    /// <response code="200">Customer history retrieved successfully</response>
    /// <response code="404">Customer not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(ApiResponse<CustomerHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerHistory(int id, [FromQuery] int limit = 50)
    {
        try
        {
            // Limit maximum results
            if (limit < 1 || limit > 200)
            {
                limit = 50;
            }

            _logger.LogInformation("Getting customer history for customer {CustomerId}, Limit: {Limit}", id, limit);

            var history = await _customerService.GetCustomerHistoryAsync(id, limit);

            if (history.Customer == null)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", id);
                return NotFound(ApiResponse<object>.Error($"Customer with ID {id} not found", StatusCodes.Status404NotFound));
            }

            _logger.LogInformation("Customer history retrieved: CustomerId={CustomerId}, Orders={OrderCount}, TotalSpent={TotalSpent}",
                id, history.TotalOrders, history.TotalSpent);

            return Ok(ApiResponse<CustomerHistoryDto>.Ok(history));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "GetCustomerHistory: Customer not found - {CustomerId}", id);
            return NotFound(ApiResponse<object>.Error($"Customer with ID {id} not found", StatusCodes.Status404NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCustomerHistory: Error retrieving history for customer {CustomerId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving customer history", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get current user ID from JWT claims
    /// </summary>
    /// <returns>User ID or null if not found</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
