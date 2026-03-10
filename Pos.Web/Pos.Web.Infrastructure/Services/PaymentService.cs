using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.UnitOfWork;
using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Models;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Payment service implementation for processing payments, discounts, and refunds
/// Implements transaction management for payment operations
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;
    private readonly IApiAuditLogService _auditLogService;
    private readonly ILogger<PaymentService> _logger;
    
    // Configuration constants (should come from configuration/database)
    private const decimal MaxDiscountPercentageWithoutApproval = 10m;
    private const decimal MaxDiscountAmountWithoutApproval = 50m;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IOrderService orderService,
        IApiAuditLogService auditLogService,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, int userId)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId} by user {UserId}", 
                request.OrderId, userId);
            
            // Get order
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(request.OrderId);
            if (order == null)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    Message = $"Order {request.OrderId} not found",
                    Errors = new List<string> { "Order not found" }
                };
            }
            
            // Validate payment amount
            var validationResult = await ValidatePaymentAsync(request.OrderId, request.AmountPaid);
            if (!validationResult.IsValid)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    Message = validationResult.Message,
                    Errors = validationResult.Errors
                };
            }
            
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Calculate change
                var changeAmount = CalculateChange(order.TotalAmount, request.AmountPaid);
                
                // Update order with payment information
                order.AmountPaid = request.AmountPaid;
                order.CustomerPaid = request.AmountPaid;
                order.ChangeAmount = changeAmount;
                order.Status = OrderStatus.Completed.ToString();
                order.CompletedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();
                
                // Record payment in payment history (if payment table exists)
                await RecordPaymentAsync(order.ID, request.PaymentMethod, request.AmountPaid, 
                    request.ReferenceNumber, userId);
                
                // Commit transaction
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("Payment processed successfully for order {OrderId}", request.OrderId);
                
                // Audit log
                await _auditLogService.LogApiRequestAsync(
                    userId: userId,
                    action: "ProcessPayment",
                    requestPath: "/api/payments",
                    requestMethod: "POST",
                    statusCode: 200,
                    duration: 0);
                
                // Get updated order
                var updatedOrder = await _orderService.GetOrderByIdAsync(request.OrderId);
                
                return new PaymentResult
                {
                    IsSuccessful = true,
                    Message = "Payment processed successfully",
                    Order = updatedOrder,
                    ChangeAmount = changeAmount
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            return new PaymentResult
            {
                IsSuccessful = false,
                Message = "Payment processing failed",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<OrderDto> ApplyDiscountAsync(ApplyDiscountRequest request, int userId)
    {
        try
        {
            _logger.LogInformation("Applying discount to order {OrderId} by user {UserId}", 
                request.OrderId, userId);
            
            // Get order
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(request.OrderId);
            if (order == null)
            {
                throw new OrderNotFoundException($"Order {request.OrderId} not found");
            }
            
            // Validate discount
            ValidateDiscount(request);
            
            // Check if manager approval is required
            if (RequiresManagerApproval(request) && !request.ApprovedBy.HasValue)
            {
                throw new PaymentValidationException(
                    "Manager approval required for this discount amount");
            }
            
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Apply discount
                order.DiscountPercentage = request.DiscountPercentage;
                order.DiscountAmount = request.DiscountAmount;
                
                // Recalculate order totals
                var calculation = await _orderService.CalculateOrderTotalsAsync(
                    order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.CategoryItemID,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList(),
                    request.DiscountPercentage,
                    request.DiscountAmount);
                
                order.Subtotal = calculation.Subtotal;
                order.TaxAmount = calculation.TaxAmount;
                order.DiscountAmount = calculation.DiscountAmount;
                order.TotalAmount = calculation.TotalAmount;
                order.TotalCost = calculation.TotalAmount;
                order.UpdatedAt = DateTime.UtcNow;
                
                // Add discount reason to notes
                order.Notes = $"{order.Notes}\nDiscount applied: {request.Reason}";
                
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();
                
                // Commit transaction
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("Discount applied successfully to order {OrderId}", request.OrderId);
                
                // Audit log
                await _auditLogService.LogApiRequestAsync(
                    userId: userId,
                    action: "ApplyDiscount",
                    requestPath: "/api/payments/discount",
                    requestMethod: "POST",
                    statusCode: 200,
                    duration: 0);
                
                // Return updated order
                return await _orderService.GetOrderByIdAsync(request.OrderId) 
                    ?? throw new InvalidOperationException("Failed to retrieve updated order");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error applying discount to order {OrderId}", request.OrderId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount to order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PaymentResult> SplitPaymentAsync(SplitPaymentRequest request, int userId)
    {
        try
        {
            _logger.LogInformation("Processing split payment for order {OrderId} by user {UserId}", 
                request.OrderId, userId);
            
            // Get order
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(request.OrderId);
            if (order == null)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    Message = $"Order {request.OrderId} not found",
                    Errors = new List<string> { "Order not found" }
                };
            }
            
            // Validate split payment
            var validationResult = ValidateSplitPayment(order, request);
            if (!validationResult.IsValid)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    Message = validationResult.Message,
                    Errors = validationResult.Errors
                };
            }
            
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Calculate total paid
                var totalPaid = request.Payments.Sum(p => p.Amount);
                var changeAmount = CalculateChange(order.TotalAmount, totalPaid);
                
                // Update order with payment information
                order.AmountPaid = totalPaid;
                order.CustomerPaid = totalPaid;
                order.ChangeAmount = changeAmount;
                order.Status = OrderStatus.Completed.ToString();
                order.CompletedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();
                
                // Record each payment in payment history
                foreach (var payment in request.Payments)
                {
                    await RecordPaymentAsync(order.ID, payment.PaymentMethod, payment.Amount, 
                        payment.ReferenceNumber, userId);
                }
                
                // Commit transaction
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("Split payment processed successfully for order {OrderId}", request.OrderId);
                
                // Audit log
                await _auditLogService.LogApiRequestAsync(
                    userId: userId,
                    action: "SplitPayment",
                    requestPath: "/api/payments/split",
                    requestMethod: "POST",
                    statusCode: 200,
                    duration: 0);
                
                // Get updated order
                var updatedOrder = await _orderService.GetOrderByIdAsync(request.OrderId);
                
                return new PaymentResult
                {
                    IsSuccessful = true,
                    Message = "Split payment processed successfully",
                    Order = updatedOrder,
                    ChangeAmount = changeAmount
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error processing split payment for order {OrderId}", request.OrderId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing split payment for order {OrderId}", request.OrderId);
            return new PaymentResult
            {
                IsSuccessful = false,
                Message = "Split payment processing failed",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public async Task<PaymentValidationResult> ValidatePaymentAsync(int orderId, decimal paymentAmount)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return new PaymentValidationResult
                {
                    IsValid = false,
                    Message = "Order not found",
                    Errors = new List<string> { "Order not found" }
                };
            }
            
            var orderTotal = order.TotalAmount;
            
            // Check if payment amount is sufficient
            if (paymentAmount < orderTotal)
            {
                var shortAmount = orderTotal - paymentAmount;
                return new PaymentValidationResult
                {
                    IsValid = false,
                    Message = $"Payment amount is insufficient. Short by {shortAmount:C}",
                    OrderTotal = orderTotal,
                    PaymentAmount = paymentAmount,
                    ShortAmount = shortAmount,
                    Errors = new List<string> { $"Payment short by {shortAmount:C}" }
                };
            }
            
            return new PaymentValidationResult
            {
                IsValid = true,
                Message = "Payment amount is valid",
                OrderTotal = orderTotal,
                PaymentAmount = paymentAmount,
                ShortAmount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment for order {OrderId}", orderId);
            return new PaymentValidationResult
            {
                IsValid = false,
                Message = "Payment validation failed",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <inheritdoc />
    public decimal CalculateChange(decimal orderTotal, decimal amountPaid)
    {
        var change = amountPaid - orderTotal;
        return change > 0 ? change : 0;
    }

    /// <inheritdoc />
    public async Task<List<PaymentDto>> GetPaymentHistoryAsync(int orderId)
    {
        try
        {
            _logger.LogDebug("Getting payment history for order {OrderId}", orderId);
            
            // TODO: Implement when payment history table is available
            // For now, return empty list
            return new List<PaymentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for order {OrderId}", orderId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RefundResult> RefundPaymentAsync(int paymentId, int userId, string reason)
    {
        try
        {
            _logger.LogInformation("Processing refund for payment {PaymentId} by user {UserId}", 
                paymentId, userId);
            
            // TODO: Implement refund logic when payment table is available
            // For now, return not implemented
            
            return new RefundResult
            {
                IsSuccessful = false,
                Message = "Refund functionality not yet implemented",
                Errors = new List<string> { "Not implemented" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
            return new RefundResult
            {
                IsSuccessful = false,
                Message = "Refund processing failed",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Record payment in payment history
    /// </summary>
    private async Task RecordPaymentAsync(int orderId, PaymentMethod paymentMethod, 
        decimal amount, string? referenceNumber, int userId)
    {
        try
        {
            // TODO: Implement when payment history table is available
            // For now, just log
            _logger.LogInformation(
                "Payment recorded: OrderId={OrderId}, Method={Method}, Amount={Amount}, User={UserId}",
                orderId, paymentMethod, amount, userId);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment for order {OrderId}", orderId);
            // Don't throw - payment was already processed
        }
    }

    /// <summary>
    /// Validate discount request
    /// </summary>
    private void ValidateDiscount(ApplyDiscountRequest request)
    {
        // Must have either percentage or amount, but not both
        if (request.DiscountPercentage.HasValue && request.DiscountAmount.HasValue)
        {
            throw new PaymentValidationException(
                "Cannot apply both percentage and fixed amount discount");
        }
        
        if (!request.DiscountPercentage.HasValue && !request.DiscountAmount.HasValue)
        {
            throw new PaymentValidationException(
                "Must specify either discount percentage or amount");
        }
        
        // Validate percentage range
        if (request.DiscountPercentage.HasValue && 
            (request.DiscountPercentage.Value < 0 || request.DiscountPercentage.Value > 100))
        {
            throw new PaymentValidationException(
                "Discount percentage must be between 0 and 100");
        }
        
        // Validate amount is positive
        if (request.DiscountAmount.HasValue && request.DiscountAmount.Value < 0)
        {
            throw new PaymentValidationException(
                "Discount amount must be positive");
        }
    }

    /// <summary>
    /// Check if discount requires manager approval
    /// </summary>
    private bool RequiresManagerApproval(ApplyDiscountRequest request)
    {
        if (request.DiscountPercentage.HasValue)
        {
            return request.DiscountPercentage.Value > MaxDiscountPercentageWithoutApproval;
        }
        
        if (request.DiscountAmount.HasValue)
        {
            return request.DiscountAmount.Value > MaxDiscountAmountWithoutApproval;
        }
        
        return false;
    }

    /// <summary>
    /// Validate split payment request
    /// </summary>
    private PaymentValidationResult ValidateSplitPayment(Order order, SplitPaymentRequest request)
    {
        var result = new PaymentValidationResult { IsValid = true };
        
        // Check minimum number of payments
        if (request.Payments.Count < 2)
        {
            result.IsValid = false;
            result.Message = "Split payment requires at least 2 payment methods";
            result.Errors.Add("Minimum 2 payment methods required");
            return result;
        }
        
        // Calculate total payment amount
        var totalPaid = request.Payments.Sum(p => p.Amount);
        
        // Check if total payment matches order total
        if (totalPaid < order.TotalAmount)
        {
            var shortAmount = order.TotalAmount - totalPaid;
            result.IsValid = false;
            result.Message = $"Total payment amount is insufficient. Short by {shortAmount:C}";
            result.OrderTotal = order.TotalAmount;
            result.PaymentAmount = totalPaid;
            result.ShortAmount = shortAmount;
            result.Errors.Add($"Payment short by {shortAmount:C}");
            return result;
        }
        
        // Validate each payment amount is positive
        foreach (var payment in request.Payments)
        {
            if (payment.Amount <= 0)
            {
                result.IsValid = false;
                result.Message = "All payment amounts must be greater than 0";
                result.Errors.Add($"Invalid amount for {payment.PaymentMethod}");
            }
        }
        
        result.OrderTotal = order.TotalAmount;
        result.PaymentAmount = totalPaid;
        result.Message = "Split payment is valid";
        
        return result;
    }
}

/// <summary>
/// Exception thrown when payment validation fails
/// </summary>
public class PaymentValidationException : Exception
{
    public PaymentValidationException(string message) : base(message) { }
}
