using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;
using System.Security.Claims;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Payments controller for processing POS payments
/// Handles payment processing, discount application, and split payments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment for an order
    /// </summary>
    /// <param name="request">Payment processing request</param>
    /// <returns>Payment result with order details and change amount</returns>
    /// <response code="200">Payment processed successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("ProcessPayment: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Processing payment for order {OrderId} by user {UserId}, Method: {PaymentMethod}, Amount: {Amount}",
                request.OrderId, userId.Value, request.PaymentMethod, request.AmountPaid);

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

            // Process payment
            var result = await _paymentService.ProcessPaymentAsync(request, userId.Value);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning("Payment processing failed for order {OrderId}: {Message}",
                    request.OrderId, result.Message);
                return BadRequest(ApiResponse<object>.Error(result.Message));
            }

            _logger.LogInformation("Payment processed successfully for order {OrderId}, Change: {Change}",
                request.OrderId, result.ChangeAmount);

            return Ok(ApiResponse<PaymentResult>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "ProcessPayment: Order not found - {OrderId}", request.OrderId);
            return NotFound(ApiResponse<object>.Error($"Order with ID {request.OrderId} not found", StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "ProcessPayment: Invalid operation for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "ProcessPayment: Invalid argument for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessPayment: Unexpected error processing payment for order {OrderId}",
                request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while processing the payment", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Apply a discount to an order
    /// </summary>
    /// <param name="request">Discount application request</param>
    /// <returns>Updated order with discount applied</returns>
    /// <response code="200">Discount applied successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - manager approval required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("discount")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("ApplyDiscount: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Applying discount to order {OrderId} by user {UserId}, Percentage: {Percentage}, Amount: {Amount}",
                request.OrderId, userId.Value, request.DiscountPercentage, request.DiscountAmount);

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

            // Validate that either percentage or amount is provided
            if (!request.DiscountPercentage.HasValue && !request.DiscountAmount.HasValue)
            {
                return BadRequest(ApiResponse<object>.Error("Either discount percentage or discount amount must be provided"));
            }

            if (request.DiscountPercentage.HasValue && request.DiscountAmount.HasValue)
            {
                return BadRequest(ApiResponse<object>.Error("Cannot apply both percentage and fixed amount discount"));
            }

            // Apply discount
            var order = await _paymentService.ApplyDiscountAsync(request, userId.Value);

            _logger.LogInformation("Discount applied successfully to order {OrderId}", request.OrderId);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "ApplyDiscount: Order not found - {OrderId}", request.OrderId);
            return NotFound(ApiResponse<object>.Error($"Order with ID {request.OrderId} not found", StatusCodes.Status404NotFound));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "ApplyDiscount: Manager approval required for order {OrderId}", request.OrderId);
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Error(ex.Message, StatusCodes.Status403Forbidden));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "ApplyDiscount: Invalid operation for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "ApplyDiscount: Invalid argument for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyDiscount: Unexpected error applying discount to order {OrderId}",
                request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while applying the discount", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Process split payment for an order (multiple payment methods)
    /// </summary>
    /// <param name="request">Split payment request</param>
    /// <returns>Payment result with order details</returns>
    /// <response code="200">Split payment processed successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("split")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SplitPayment([FromBody] SplitPaymentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("SplitPayment: User ID not found in claims");
                return Unauthorized(ApiResponse<object>.Error("User authentication failed"));
            }

            _logger.LogInformation("Processing split payment for order {OrderId} by user {UserId}, Payment count: {Count}",
                request.OrderId, userId.Value, request.Payments.Count);

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

            // Validate payment count
            if (request.Payments == null || request.Payments.Count < 2)
            {
                return BadRequest(ApiResponse<object>.Error("Split payment requires at least 2 payment methods"));
            }

            // Process split payment
            var result = await _paymentService.SplitPaymentAsync(request, userId.Value);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning("Split payment processing failed for order {OrderId}: {Message}",
                    request.OrderId, result.Message);
                return BadRequest(ApiResponse<object>.Error(result.Message));
            }

            _logger.LogInformation("Split payment processed successfully for order {OrderId}", request.OrderId);
            return Ok(ApiResponse<PaymentResult>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "SplitPayment: Order not found - {OrderId}", request.OrderId);
            return NotFound(ApiResponse<object>.Error($"Order with ID {request.OrderId} not found", StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "SplitPayment: Invalid operation for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "SplitPayment: Invalid argument for order {OrderId} - {Message}",
                request.OrderId, ex.Message);
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SplitPayment: Unexpected error processing split payment for order {OrderId}",
                request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Error("An error occurred while processing the split payment", StatusCodes.Status500InternalServerError));
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
