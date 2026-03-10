using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.Enums;
using System.Security.Claims;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Kitchen controller for managing kitchen display and order status updates
/// Provides endpoints for kitchen staff to view and update order preparation status
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KitchenController : ControllerBase
{
    private readonly IKitchenService _kitchenService;
    private readonly ILogger<KitchenController> _logger;

    public KitchenController(
        IKitchenService kitchenService,
        ILogger<KitchenController> logger)
    {
        _kitchenService = kitchenService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active orders for kitchen display
    /// Returns orders with Pending and Preparing statuses, sorted by priority
    /// </summary>
    /// <returns>List of active kitchen orders</returns>
    /// <response code="200">Returns list of active orders</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<KitchenOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveOrders()
    {
        try
        {
            _logger.LogInformation("Getting active kitchen orders");

            var orders = await _kitchenService.GetActiveOrdersAsync();

            _logger.LogInformation("Retrieved {Count} active kitchen orders", orders.Count);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active kitchen orders");
            return StatusCode(500, new { message = "An error occurred while retrieving kitchen orders" });
        }
    }

    /// <summary>
    /// Update order status
    /// Valid transitions: Pending -> Preparing -> Ready -> Delivered
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated order information</returns>
    /// <response code="200">Order status updated successfully</response>
    /// <response code="400">Invalid status transition or bad request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("orders/{id}/status")]
    [ProducesResponseType(typeof(KitchenOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid order ID" });
            }

            if (request == null || !Enum.IsDefined(typeof(OrderStatus), request.NewStatus))
            {
                return BadRequest(new { message = "Invalid status value" });
            }

            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID in token" });
            }

            _logger.LogInformation(
                "Updating order {OrderId} status to {NewStatus} by user {UserId}",
                id, request.NewStatus, userId);

            var updatedOrder = await _kitchenService.UpdateOrderStatusAsync(id, request.NewStatus, userId);

            _logger.LogInformation(
                "Order {OrderId} status updated successfully to {NewStatus}",
                id, request.NewStatus);

            return Ok(updatedOrder);
        }
        catch (InvalidOrderStatusTransitionException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid status transition for order {OrderId}: {CurrentStatus} -> {AttemptedStatus}",
                id, ex.CurrentStatus, ex.AttemptedStatus);

            return BadRequest(new
            {
                message = ex.Message,
                currentStatus = ex.CurrentStatus.ToString(),
                attemptedStatus = ex.AttemptedStatus.ToString()
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", id);
            return StatusCode(500, new { message = "An error occurred while updating order status" });
        }
    }

    /// <summary>
    /// Get order history filtered by status and optional date range
    /// </summary>
    /// <param name="status">Order status to filter by (optional)</param>
    /// <param name="fromDate">Start date (optional, format: yyyy-MM-dd)</param>
    /// <param name="toDate">End date (optional, format: yyyy-MM-dd)</param>
    /// <returns>List of orders matching the criteria</returns>
    /// <response code="200">Returns filtered order list</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders/history")]
    [ProducesResponseType(typeof(List<KitchenOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderHistory(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest(new { message = "fromDate cannot be greater than toDate" });
            }

            _logger.LogInformation(
                "Getting order history with status={Status}, fromDate={FromDate}, toDate={ToDate}",
                status, fromDate, toDate);

            List<KitchenOrderDto> orders;

            if (status.HasValue)
            {
                orders = await _kitchenService.GetOrdersByStatusWithFilterAsync(
                    status.Value,
                    fromDate,
                    toDate);
            }
            else
            {
                // If no status specified, get all orders for today by default
                var today = DateTime.Today;
                orders = await _kitchenService.GetOrdersByStatusWithFilterAsync(
                    OrderStatus.Delivered,
                    fromDate ?? today,
                    toDate ?? today.AddDays(1).AddSeconds(-1));
            }

            _logger.LogInformation("Retrieved {Count} orders from history", orders.Count);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order history");
            return StatusCode(500, new { message = "An error occurred while retrieving order history" });
        }
    }

    /// <summary>
    /// Get overdue orders (orders waiting too long)
    /// </summary>
    /// <param name="thresholdMinutes">Minutes threshold for overdue (default 30)</param>
    /// <returns>List of overdue orders</returns>
    /// <response code="200">Returns list of overdue orders</response>
    /// <response code="400">Invalid threshold value</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders/overdue")]
    [ProducesResponseType(typeof(List<KitchenOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOverdueOrders([FromQuery] int thresholdMinutes = 30)
    {
        try
        {
            if (thresholdMinutes <= 0 || thresholdMinutes > 1440) // Max 24 hours
            {
                return BadRequest(new { message = "Threshold must be between 1 and 1440 minutes" });
            }

            _logger.LogInformation("Getting overdue orders with threshold {ThresholdMinutes} minutes", thresholdMinutes);

            var orders = await _kitchenService.GetOverdueOrdersAsync(thresholdMinutes);

            _logger.LogInformation("Retrieved {Count} overdue orders", orders.Count);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue orders");
            return StatusCode(500, new { message = "An error occurred while retrieving overdue orders" });
        }
    }

    /// <summary>
    /// Get kitchen statistics for today
    /// </summary>
    /// <returns>Kitchen statistics including order counts and average times</returns>
    /// <response code="200">Returns kitchen statistics</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(KitchenStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            _logger.LogInformation("Getting kitchen statistics for today");

            var statistics = await _kitchenService.GetTodayStatisticsAsync();

            _logger.LogInformation(
                "Retrieved kitchen statistics: {TotalOrders} total, {PendingOrders} pending, {PreparingOrders} preparing",
                statistics.TotalOrdersToday, statistics.PendingOrders, statistics.PreparingOrders);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving kitchen statistics" });
        }
    }

    /// <summary>
    /// Mark order items as started preparation
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Request containing item IDs to mark as started</param>
    /// <returns>Success response</returns>
    /// <response code="200">Items marked as started successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("orders/{id}/items/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkItemsAsStarted(int id, [FromBody] MarkItemsRequest request)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid order ID" });
            }

            if (request == null || request.ItemIds == null || !request.ItemIds.Any())
            {
                return BadRequest(new { message = "Item IDs are required" });
            }

            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID in token" });
            }

            _logger.LogInformation(
                "Marking {Count} items as started for order {OrderId} by user {UserId}",
                request.ItemIds.Count, id, userId);

            await _kitchenService.MarkItemsAsStartedAsync(id, request.ItemIds, userId);

            _logger.LogInformation("Items marked as started successfully for order {OrderId}", id);

            return Ok(new { message = "Items marked as started successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking items as started for order {OrderId}", id);
            return StatusCode(500, new { message = "An error occurred while marking items as started" });
        }
    }

    /// <summary>
    /// Mark order items as completed
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Request containing item IDs to mark as completed</param>
    /// <returns>Success response</returns>
    /// <response code="200">Items marked as completed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("orders/{id}/items/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkItemsAsCompleted(int id, [FromBody] MarkItemsRequest request)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid order ID" });
            }

            if (request == null || request.ItemIds == null || !request.ItemIds.Any())
            {
                return BadRequest(new { message = "Item IDs are required" });
            }

            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID in token" });
            }

            _logger.LogInformation(
                "Marking {Count} items as completed for order {OrderId} by user {UserId}",
                request.ItemIds.Count, id, userId);

            await _kitchenService.MarkItemsAsCompletedAsync(id, request.ItemIds, userId);

            _logger.LogInformation("Items marked as completed successfully for order {OrderId}", id);

            return Ok(new { message = "Items marked as completed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking items as completed for order {OrderId}", id);
            return StatusCode(500, new { message = "An error occurred while marking items as completed" });
        }
    }

    /// <summary>
    /// Invalidate kitchen cache
    /// Use this endpoint when orders are updated externally to refresh the kitchen display
    /// </summary>
    /// <returns>Success response</returns>
    /// <response code="200">Cache invalidated successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("cache/invalidate")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InvalidateCache()
    {
        try
        {
            _logger.LogInformation("Invalidating kitchen cache");

            await _kitchenService.InvalidateCacheAsync();

            _logger.LogInformation("Kitchen cache invalidated successfully");

            return Ok(new { message = "Kitchen cache invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating kitchen cache");
            return StatusCode(500, new { message = "An error occurred while invalidating cache" });
        }
    }
}

/// <summary>
/// Request model for updating order status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// New status to set
    /// Valid values: Pending, Preparing, Ready, Delivered
    /// </summary>
    public OrderStatus NewStatus { get; set; }
}

/// <summary>
/// Request model for marking items as started or completed
/// </summary>
public class MarkItemsRequest
{
    /// <summary>
    /// List of order item IDs to mark
    /// </summary>
    public List<int> ItemIds { get; set; } = new();
}
