using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;
using System.Security.Claims;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Orders controller for managing POS orders
/// Handles order creation, retrieval, updates, and split operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order with ID</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("CreateOrder: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Creating order for user {UserId}, ServiceType: {ServiceType}, Items: {ItemCount}",
                userId.Value, request.ServiceType, request.Items.Count);

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

            // Create order
            var order = await _orderService.CreateOrderAsync(request, userId.Value);

            _logger.LogInformation("Order created successfully: OrderId={OrderId}, Total={Total}, UserId={UserId}",
                order.Id, order.TotalAmount, userId.Value);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                ApiResponse<OrderDto>.Ok(order, StatusCodes.Status201Created));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CreateOrder: Invalid operation - {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "CreateOrder: Invalid argument - {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateOrder: Unexpected error creating order for user {UserId}", GetCurrentUserId());
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while creating the order", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            _logger.LogInformation("Getting order by ID: {OrderId}", id);

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", id);
                return NotFound(ApiResponse<object>.Error($"Order with ID {id} not found", StatusCodes.Status404NotFound));
            }

            _logger.LogInformation("Order retrieved successfully: {OrderId}", id);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrderById: Error retrieving order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving the order", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Update an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Order update request</param>
    /// <returns>Updated order</returns>
    /// <response code="200">Order updated successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Order not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="409">Order is locked by another user</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("UpdateOrder: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            // Validate ID match
            if (id != request.OrderId)
            {
                _logger.LogWarning("UpdateOrder: ID mismatch - URL: {UrlId}, Body: {BodyId}", id, request.OrderId);
                return BadRequest(ApiResponse<object>.Error("Order ID in URL does not match request body"));
            }

            _logger.LogInformation("Updating order {OrderId} by user {UserId}", id, userId.Value);

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

            // Update order
            var order = await _orderService.UpdateOrderAsync(request, userId.Value);

            _logger.LogInformation("Order updated successfully: {OrderId}", id);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "UpdateOrder: Invalid operation for order {OrderId} - {Message}", id, ex.Message);
            
            // Check if it's a lock-related error
            if (ex.Message.Contains("locked") || ex.Message.Contains("being edited"))
            {
                return Conflict(ApiResponse<object>.Error(ex.Message, StatusCodes.Status409Conflict));
            }
            
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "UpdateOrder: Order not found - {OrderId}", id);
            return NotFound(ApiResponse<object>.Error($"Order with ID {id} not found", StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "UpdateOrder: Invalid argument for order {OrderId} - {Message}", id, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateOrder: Unexpected error updating order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while updating the order", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get all pending orders (not completed or cancelled)
    /// </summary>
    /// <param name="userId">Optional: Filter by user ID</param>
    /// <param name="tableNumber">Optional: Filter by table number</param>
    /// <returns>List of pending orders</returns>
    /// <response code="200">Pending orders retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPendingOrders([FromQuery] int? userId = null, [FromQuery] byte? tableNumber = null)
    {
        try
        {
            _logger.LogInformation("Getting pending orders - UserId: {UserId}, TableNumber: {TableNumber}",
                userId, tableNumber);

            var orders = await _orderService.GetPendingOrdersAsync(userId, tableNumber);

            _logger.LogInformation("Retrieved {Count} pending orders", orders.Count);
            return Ok(ApiResponse<List<OrderDto>>.Ok(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPendingOrders: Error retrieving pending orders");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while retrieving pending orders", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Split an order into multiple orders
    /// </summary>
    /// <param name="id">Original order ID to split</param>
    /// <param name="splitRequests">List of new orders to create from split</param>
    /// <returns>List of created split orders</returns>
    /// <response code="201">Orders split successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Original order not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id}/split")]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SplitOrder(int id, [FromBody] List<CreateOrderRequest> splitRequests)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("SplitOrder: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Splitting order {OrderId} into {Count} orders by user {UserId}",
                id, splitRequests.Count, userId.Value);

            // Validate request
            if (splitRequests == null || splitRequests.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Error("Split requests cannot be empty"));
            }

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

            // Split order
            var orders = await _orderService.SplitOrderAsync(id, splitRequests, userId.Value);

            _logger.LogInformation("Order {OrderId} split successfully into {Count} orders", id, orders.Count);
            return CreatedAtAction(
                nameof(GetPendingOrders),
                null,
                ApiResponse<List<OrderDto>>.Ok(orders, StatusCodes.Status201Created));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "SplitOrder: Invalid operation for order {OrderId} - {Message}", id, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "SplitOrder: Order not found - {OrderId}", id);
            return NotFound(ApiResponse<object>.Error($"Order with ID {id} not found", StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "SplitOrder: Invalid argument for order {OrderId} - {Message}", id, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SplitOrder: Unexpected error splitting order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while splitting the order", StatusCodes.Status500InternalServerError));
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
