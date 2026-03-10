using Pos.Web.Shared.DTOs;
using Pos.Web.Shared.Enums;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Kitchen service interface for managing kitchen display and order status updates
/// Provides real-time order tracking for kitchen staff
/// </summary>
public interface IKitchenService
{
    /// <summary>
    /// Get all active orders for kitchen display (Pending and Preparing statuses)
    /// Orders are sorted by priority (wait time, service type, etc.)
    /// Results are cached for 30 seconds
    /// </summary>
    /// <returns>List of active orders with priority information</returns>
    Task<List<KitchenOrderDto>> GetActiveOrdersAsync();

    /// <summary>
    /// Update order status with validation
    /// Valid transitions: Pending -> Preparing -> Ready -> Delivered
    /// </summary>
    /// <param name="orderId">Order ID to update</param>
    /// <param name="newStatus">New status to set</param>
    /// <param name="userId">User ID making the update</param>
    /// <returns>Updated order information</returns>
    /// <exception cref="InvalidOrderStatusTransitionException">Thrown when status transition is invalid</exception>
    Task<KitchenOrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int userId);

    /// <summary>
    /// Get orders filtered by status
    /// </summary>
    /// <param name="status">Order status to filter by</param>
    /// <returns>List of orders with the specified status</returns>
    Task<List<KitchenOrderDto>> GetOrdersByStatusAsync(OrderStatus status);

    /// <summary>
    /// Get orders filtered by status with date range
    /// </summary>
    /// <param name="status">Order status to filter by</param>
    /// <param name="fromDate">Start date (optional)</param>
    /// <param name="toDate">End date (optional)</param>
    /// <returns>List of orders matching the criteria</returns>
    Task<List<KitchenOrderDto>> GetOrdersByStatusWithFilterAsync(
        OrderStatus status, 
        DateTime? fromDate = null, 
        DateTime? toDate = null);

    /// <summary>
    /// Calculate order priority based on wait time, service type, and other factors
    /// Higher priority = more urgent
    /// </summary>
    /// <param name="orderId">Order ID to calculate priority for</param>
    /// <returns>Priority score (0-100)</returns>
    Task<int> CalculateOrderPriorityAsync(int orderId);

    /// <summary>
    /// Get orders that are overdue (waiting too long)
    /// </summary>
    /// <param name="thresholdMinutes">Minutes threshold for overdue (default 30)</param>
    /// <returns>List of overdue orders</returns>
    Task<List<KitchenOrderDto>> GetOverdueOrdersAsync(int thresholdMinutes = 30);

    /// <summary>
    /// Get kitchen statistics for today
    /// </summary>
    /// <returns>Kitchen statistics</returns>
    Task<KitchenStatisticsDto> GetTodayStatisticsAsync();

    /// <summary>
    /// Mark order items as started preparation
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="itemIds">List of order item IDs to mark as started</param>
    /// <param name="userId">User ID making the update</param>
    Task MarkItemsAsStartedAsync(int orderId, List<int> itemIds, int userId);

    /// <summary>
    /// Mark order items as completed
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="itemIds">List of order item IDs to mark as completed</param>
    /// <param name="userId">User ID making the update</param>
    Task MarkItemsAsCompletedAsync(int orderId, List<int> itemIds, int userId);

    /// <summary>
    /// Invalidate kitchen cache
    /// Call this when orders are updated externally
    /// </summary>
    Task InvalidateCacheAsync();
}

/// <summary>
/// Kitchen order DTO with priority and timing information
/// </summary>
public class KitchenOrderDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public ServiceType ServiceType { get; set; }
    public byte? TableNumber { get; set; }
    public OrderStatus Status { get; set; }
    public List<KitchenOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Kitchen-specific fields
    public int Priority { get; set; }
    public TimeSpan WaitTime { get; set; }
    public bool IsOverdue { get; set; }
    public string? PreparedByUserName { get; set; }
}

/// <summary>
/// Kitchen order item DTO
/// </summary>
public class KitchenOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Kitchen statistics DTO
/// </summary>
public class KitchenStatisticsDto
{
    public int TotalOrdersToday { get; set; }
    public int PendingOrders { get; set; }
    public int PreparingOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int OverdueOrders { get; set; }
    public TimeSpan AveragePreparationTime { get; set; }
    public TimeSpan AverageWaitTime { get; set; }
}

/// <summary>
/// Exception thrown when an invalid order status transition is attempted
/// </summary>
public class InvalidOrderStatusTransitionException : Exception
{
    public OrderStatus CurrentStatus { get; }
    public OrderStatus AttemptedStatus { get; }

    public InvalidOrderStatusTransitionException(
        string message, 
        OrderStatus currentStatus, 
        OrderStatus attemptedStatus) : base(message)
    {
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }
}
