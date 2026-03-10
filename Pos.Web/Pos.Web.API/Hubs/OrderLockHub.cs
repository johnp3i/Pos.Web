using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pos.Web.Infrastructure.Services;
using Pos.Web.Shared.Constants;
using Pos.Web.Shared.Messages;
using System.Security.Claims;

namespace Pos.Web.API.Hubs;

/// <summary>
/// SignalR hub for real-time order lock notifications
/// Prevents concurrent order modifications by notifying users when orders are locked/unlocked
/// </summary>
[Authorize]
public class OrderLockHub : Hub
{
    private readonly IOrderLockService _orderLockService;
    private readonly ILogger<OrderLockHub> _logger;

    public OrderLockHub(
        IOrderLockService orderLockService,
        ILogger<OrderLockHub> logger)
    {
        _orderLockService = orderLockService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userName = GetUserName();
        
        _logger.LogInformation("User {UserName} (ID: {UserId}) connected to OrderLockHub. ConnectionId: {ConnectionId}", 
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
        
        // Release any locks held by this user
        try
        {
            var userLocks = await _orderLockService.GetUserLocksAsync(userId);
            foreach (var lockStatus in userLocks)
            {
                await _orderLockService.ReleaseLockAsync(lockStatus.OrderId, userId);
                
                // Notify others that lock was released
                var message = new OrderUnlockedMessage
                {
                    OrderId = lockStatus.OrderId,
                    UnlockedBy = userId,
                    UnlockedByName = userName,
                    Timestamp = DateTime.UtcNow
                };
                
                await Clients.Others.SendAsync(SignalRMethods.OrderLock.OrderUnlocked, message);
            }
            
            if (userLocks.Any())
            {
                _logger.LogInformation("Released {Count} locks for disconnected user {UserName} (ID: {UserId})", 
                    userLocks.Count, userName, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing locks for disconnected user {UserName} (ID: {UserId})", 
                userName, userId);
        }
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserName} (ID: {UserId}) disconnected from OrderLockHub with error. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User {UserName} (ID: {UserId}) disconnected from OrderLockHub. ConnectionId: {ConnectionId}", 
                userName, userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Acquire a lock on an order
    /// </summary>
    /// <param name="orderId">Order ID to lock</param>
    /// <param name="lockDurationMinutes">Lock duration in minutes (default 5)</param>
    /// <returns>True if lock acquired, false if already locked by another user</returns>
    public async Task<bool> AcquireLock(int orderId, int lockDurationMinutes = 5)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) attempting to acquire lock on order {OrderId}", 
                userName, userId, orderId);
            
            // Attempt to acquire lock
            var lockResult = await _orderLockService.AcquireLockAsync(
                orderId, 
                userId, 
                sessionId: Context.ConnectionId, 
                deviceInfo: Context.GetHttpContext()?.Request.Headers.UserAgent.ToString(),
                timeoutMinutes: lockDurationMinutes);
            
            if (!lockResult.Success)
            {
                _logger.LogWarning("User {UserName} (ID: {UserId}) failed to acquire lock on order {OrderId}: {Reason}", 
                    userName, userId, orderId, lockResult.ErrorMessage);
                
                // Notify caller of failure
                await Clients.Caller.SendAsync("LockAcquisitionFailed", new
                {
                    OrderId = orderId,
                    Error = lockResult.ErrorMessage,
                    LockedBy = lockResult.ExistingLock?.LockedByUserId,
                    LockedByName = lockResult.ExistingLock?.LockedByUserName,
                    LockExpiresAt = lockResult.ExistingLock?.LockExpiresAt
                });
                
                return false;
            }
            
            // Create lock notification message
            var message = new OrderLockedMessage
            {
                OrderId = orderId,
                LockedBy = userId,
                LockedByName = userName,
                LockExpiresAt = lockResult.ExpiresAt!.Value,
                Timestamp = DateTime.UtcNow
            };
            
            // Broadcast to all clients except caller
            await Clients.Others.SendAsync(SignalRMethods.OrderLock.OrderLocked, message);
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) acquired lock on order {OrderId} until {ExpiresAt}", 
                userName, userId, orderId, lockResult.ExpiresAt);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock on order {OrderId}", orderId);
            
            await Clients.Caller.SendAsync("LockAcquisitionFailed", new
            {
                OrderId = orderId,
                Error = "An error occurred while acquiring lock"
            });
            
            return false;
        }
    }

    /// <summary>
    /// Release a lock on an order
    /// </summary>
    /// <param name="orderId">Order ID to unlock</param>
    /// <returns>True if lock released, false if no lock exists or not owned by user</returns>
    public async Task<bool> ReleaseLock(int orderId)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) attempting to release lock on order {OrderId}", 
                userName, userId, orderId);
            
            // Attempt to release lock
            var releaseResult = await _orderLockService.ReleaseLockAsync(orderId, userId);
            
            if (!releaseResult)
            {
                _logger.LogWarning("User {UserName} (ID: {UserId}) failed to release lock on order {OrderId}: No lock found or not owned by user", 
                    userName, userId, orderId);
                
                await Clients.Caller.SendAsync("LockReleaseFailed", new
                {
                    OrderId = orderId,
                    Error = "No lock found or you don't own this lock"
                });
                
                return false;
            }
            
            // Create unlock notification message
            var message = new OrderUnlockedMessage
            {
                OrderId = orderId,
                UnlockedBy = userId,
                UnlockedByName = userName,
                Timestamp = DateTime.UtcNow
            };
            
            // Broadcast to all clients except caller
            await Clients.Others.SendAsync(SignalRMethods.OrderLock.OrderUnlocked, message);
            
            _logger.LogInformation("User {UserName} (ID: {UserId}) released lock on order {OrderId}", 
                userName, userId, orderId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock on order {OrderId}", orderId);
            
            await Clients.Caller.SendAsync("LockReleaseFailed", new
            {
                OrderId = orderId,
                Error = "An error occurred while releasing lock"
            });
            
            return false;
        }
    }

    /// <summary>
    /// Check if an order is currently locked
    /// </summary>
    /// <param name="orderId">Order ID to check</param>
    /// <returns>Lock status information</returns>
    public async Task<object> GetLockStatus(int orderId)
    {
        try
        {
            var lockStatus = await _orderLockService.GetLockStatusAsync(orderId);
            
            if (lockStatus == null || !lockStatus.IsLocked)
            {
                return new
                {
                    IsLocked = false,
                    OrderId = orderId
                };
            }
            
            return new
            {
                IsLocked = true,
                OrderId = orderId,
                LockedBy = lockStatus.LockedByUserId,
                LockedByName = lockStatus.LockedByUserName,
                LockExpiresAt = lockStatus.LockExpiresAt,
                RemainingSeconds = lockStatus.TimeRemaining?.TotalSeconds ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lock status for order {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Notify all clients that a lock has expired
    /// This is typically called by a background service
    /// </summary>
    /// <param name="orderId">Order ID whose lock expired</param>
    /// <param name="userId">User ID who held the lock</param>
    public async Task NotifyLockExpired(int orderId, int userId)
    {
        try
        {
            _logger.LogInformation("Lock expired for order {OrderId}, previously held by user {UserId}", 
                orderId, userId);
            
            // Broadcast to all clients
            await Clients.All.SendAsync(SignalRMethods.OrderLock.LockExpired, new
            {
                OrderId = orderId,
                PreviouslyLockedBy = userId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying lock expiration for order {OrderId}", orderId);
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
