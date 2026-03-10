using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Models;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Order service interface for managing order operations
/// Handles order CRUD, validation, stock checking, and order locking integration
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new order with validation and stock checking
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="userId">ID of the user creating the order</param>
    /// <returns>Created order DTO</returns>
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int userId);
    
    /// <summary>
    /// Update an existing order with order locking
    /// </summary>
    /// <param name="request">Order update request</param>
    /// <param name="userId">ID of the user updating the order</param>
    /// <returns>Updated order DTO</returns>
    Task<OrderDto> UpdateOrderAsync(UpdateOrderRequest request, int userId);
    
    /// <summary>
    /// Get order by ID with all related data
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order DTO or null if not found</returns>
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    
    /// <summary>
    /// Get all pending orders (not completed)
    /// </summary>
    /// <param name="userId">Optional user ID to filter by</param>
    /// <param name="tableNumber">Optional table number to filter by</param>
    /// <returns>List of pending orders</returns>
    Task<List<OrderDto>> GetPendingOrdersAsync(int? userId = null, byte? tableNumber = null);
    
    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="limit">Maximum number of orders to return</param>
    /// <returns>List of customer orders</returns>
    Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId, int limit = 20);
    
    /// <summary>
    /// Get today's orders
    /// </summary>
    /// <param name="userId">Optional user ID to filter by</param>
    /// <returns>List of today's orders</returns>
    Task<List<OrderDto>> GetTodaysOrdersAsync(int? userId = null);
    
    /// <summary>
    /// Get orders by date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="userId">Optional user ID to filter by</param>
    /// <returns>List of orders in date range</returns>
    Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate, int? userId = null);
    
    /// <summary>
    /// Split an order into multiple orders
    /// </summary>
    /// <param name="orderId">Original order ID</param>
    /// <param name="splitRequests">List of split order requests</param>
    /// <param name="userId">ID of the user performing the split</param>
    /// <returns>List of created split orders</returns>
    Task<List<OrderDto>> SplitOrderAsync(int orderId, List<CreateOrderRequest> splitRequests, int userId);
    
    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="orderId">Order ID to cancel</param>
    /// <param name="userId">ID of the user canceling the order</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>True if canceled successfully</returns>
    Task<bool> CancelOrderAsync(int orderId, int userId, string? reason = null);
    
    /// <summary>
    /// Complete an order (mark as paid and completed)
    /// </summary>
    /// <param name="orderId">Order ID to complete</param>
    /// <param name="userId">ID of the user completing the order</param>
    /// <returns>Completed order DTO</returns>
    Task<OrderDto> CompleteOrderAsync(int orderId, int userId);
    
    /// <summary>
    /// Validate order items (stock availability, pricing)
    /// </summary>
    /// <param name="items">Order items to validate</param>
    /// <returns>Validation result with errors if any</returns>
    Task<OrderValidationResult> ValidateOrderItemsAsync(List<OrderItemDto> items);
    
    /// <summary>
    /// Calculate order totals (subtotal, tax, discounts, total)
    /// </summary>
    /// <param name="items">Order items</param>
    /// <param name="discountPercentage">Discount percentage (0-100)</param>
    /// <param name="discountAmount">Fixed discount amount</param>
    /// <returns>Order calculation result</returns>
    Task<OrderCalculationResult> CalculateOrderTotalsAsync(
        List<OrderItemDto> items, 
        decimal? discountPercentage = null, 
        decimal? discountAmount = null);
}

/// <summary>
/// Order validation result
/// </summary>
public class OrderValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<int, string> ItemErrors { get; set; } = new(); // ProductId -> Error message
}

/// <summary>
/// Order calculation result
/// </summary>
public class OrderCalculationResult
{
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
