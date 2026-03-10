using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.Constants;
using Pos.Web.Shared.Enums;
using Pos.Web.Shared.Messages;
using System.Security.Claims;

namespace Pos.Web.API.Hubs;

/// <summary>
/// SignalR hub for real-time kitchen order management
/// </summary>
[Authorize]
public class KitchenHub : Hub
{
    private readonly IKitchenService _kitchenService;
    private readonly ILogger<KitchenHub> _logger;
    private const string KitchenGroupName = "Kitchen";

    public KitchenHub(
        IKitchenService kitchenService,
        ILogger<KitchenHub> logger)
    {
        _kitchenService = kitchenService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        _logger.LogInformation("User {UserName} (ID: {UserId}) connected to KitchenHub. ConnectionId: {ConnectionId}", 
            userName, userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserName} (ID: {UserId}) disconnected from KitchenHub with error. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User {UserName} (ID: {UserId}) disconnected from KitchenHub. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join the kitchen group to receive order notifications
    /// </summary>
    public async Task JoinKitchenGroup()
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, KitchenGroupName);
            
            var userName = GetUserName();
            _logger.LogInformation("User {UserName} joined kitchen group. ConnectionId: {ConnectionId}", 
                userName, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining kitchen group. ConnectionId: {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    /// <summary>
    /// Leave the kitchen group
    /// </summary>
    public async Task LeaveKitchenGroup()
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, KitchenGroupName);
            
            var userName = GetUserName();
            _logger.LogInformation("User {UserName} left kitchen group. ConnectionId: {ConnectionId}", 
                userName, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving kitchen group. ConnectionId: {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    /// <summary>
    /// Send a new order to all kitchen displays
    /// </summary>
    /// <param name="message">Kitchen order message</param>
    public async Task SendOrderToKitchen(KitchenOrderMessage message)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) sending order {OrderId} to kitchen", 
                userName, userId, message.OrderId);
            
            // Broadcast to all clients in the kitchen group
            await Clients.Group(KitchenGroupName).SendAsync(
                SignalRMethods.Kitchen.NewOrderReceived, 
                message);
            
            _logger.LogInformation("Order {OrderId} sent to kitchen group successfully", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order {OrderId} to kitchen", message.OrderId);
            throw;
        }
    }

    /// <summary>
    /// Update order status and notify all kitchen displays
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New order status</param>
    public async Task UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) updating order {OrderId} status to {Status}", 
                userName, userId, orderId, newStatus);
            
            // Update status via service
            var updatedOrder = await _kitchenService.UpdateOrderStatusAsync(orderId, newStatus, userId);
            
            // Get the old status from the updated order (we'll need to track this differently in production)
            // For now, we'll use the current status as old status (this is a simplification)
            var oldStatus = updatedOrder.Status == newStatus 
                ? (OrderStatus)((int)newStatus - 1) 
                : updatedOrder.Status;
            
            // Create status change message
            var message = new OrderStatusChangedMessage
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = userId,
                ChangedByName = userName,
                Timestamp = DateTime.UtcNow
            };
            
            // Broadcast to all clients in the kitchen group
            await Clients.Group(KitchenGroupName).SendAsync(
                SignalRMethods.Kitchen.OrderStatusChanged, 
                message);
            
            _logger.LogInformation("Order {OrderId} status updated to {Status} and broadcast to kitchen group", 
                orderId, newStatus);
        }
        catch (InvalidOrderStatusTransitionException ex)
        {
            _logger.LogWarning(ex, "Invalid status transition for order {OrderId}: {CurrentStatus} -> {AttemptedStatus}", 
                orderId, ex.CurrentStatus, ex.AttemptedStatus);
            
            // Notify caller of invalid transition
            await Clients.Caller.SendAsync("UpdateOrderStatusFailed", new
            {
                OrderId = orderId,
                Error = ex.Message,
                CurrentStatus = ex.CurrentStatus,
                AttemptedStatus = ex.AttemptedStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, newStatus);
            
            // Notify caller of error
            await Clients.Caller.SendAsync("UpdateOrderStatusFailed", new
            {
                OrderId = orderId,
                Error = "An error occurred while updating order status"
            });
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get current user name from claims
    /// </summary>
    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }
}
