# Phase 9 Complete: SignalR Hubs

## Overview
Phase 9 implemented all three SignalR hubs for real-time bidirectional communication between the server and clients. These hubs enable kitchen displays, order locking, and device command distribution.

## Completion Date
March 6, 2026

## Tasks Completed

### ✅ 9.1 KitchenHub
**File**: `Pos.Web.API/Hubs/KitchenHub.cs`
**Endpoint**: `/hubs/kitchen`

**Features**:
- Connection lifecycle management
- Kitchen group management (join/leave)
- Send orders to kitchen displays
- Update order status with validation
- Broadcast status changes to all kitchen displays
- User tracking via JWT claims

**Key Methods**:
- `JoinKitchenGroup()` - Subscribe to kitchen notifications
- `LeaveKitchenGroup()` - Unsubscribe from kitchen notifications
- `SendOrderToKitchen(KitchenOrderMessage)` - Broadcast new order
- `UpdateOrderStatus(orderId, newStatus)` - Update and broadcast status

### ✅ 9.2 OrderLockHub
**File**: `Pos.Web.API/Hubs/OrderLockHub.cs`
**Endpoint**: `/hubs/orderlock`

**Features**:
- Order lock acquisition and release
- Real-time lock notifications
- Automatic lock release on disconnect
- Lock status checking
- Lock expiration notifications

**Key Methods**:
- `AcquireLock(orderId, lockDurationMinutes)` - Acquire order lock
- `ReleaseLock(orderId)` - Release order lock
- `GetLockStatus(orderId)` - Check lock status
- `NotifyLockExpired(orderId, userId)` - Broadcast lock expiration

**Auto-Cleanup**:
- Releases all user locks on disconnect
- Notifies other clients of lock releases

### ✅ 9.3 ServerCommandHub
**File**: `Pos.Web.API/Hubs/ServerCommandHub.cs`
**Endpoint**: `/hubs/servercommand`

**Features**:
- Device registration as master stations
- Command queue management (in-memory)
- Print command distribution
- Cash drawer commands
- Command status tracking
- Device connection tracking

**Key Methods**:
- `RegisterDevice(deviceId)` - Register as command receiver
- `UnregisterDevice(deviceId)` - Unregister device
- `SendCommand(ServerCommandMessage)` - Send command to device
- `SendPrintCommand(deviceId, printData)` - Send print command
- `SendCashDrawerCommand(deviceId)` - Send cash drawer open command
- `NotifyCommandCompleted(commandId, result)` - Mark command complete
- `NotifyCommandFailed(commandId, errorMessage)` - Mark command failed
- `GetCommandStatus(commandId)` - Check command status
- `GetPendingCommands(deviceId)` - Get queued commands
- `GetRegisteredDevices()` - List connected devices

## Hub Configuration

### Program.cs Updates
```csharp
// Hub mappings added
app.MapHub<KitchenHub>("/hubs/kitchen");
app.MapHub<OrderLockHub>("/hubs/orderlock");
app.MapHub<ServerCommandHub>("/hubs/servercommand");
```

### SignalR Configuration (Existing)
```csharp
services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 128 * 1024; // 128 KB
});
```

## Common Patterns

### Authentication
All hubs require JWT authentication via `[Authorize]` attribute:
```csharp
[Authorize]
public class KitchenHub : Hub
```

### User Context
All hubs extract user information from JWT claims:
```csharp
private int GetUserId()
{
    var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return int.TryParse(userIdClaim, out var userId) ? userId : 0;
}

private string GetUserName()
{
    return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
}
```

### Connection Lifecycle
All hubs implement connection/disconnection logging:
```csharp
public override async Task OnConnectedAsync()
{
    _logger.LogInformation("User connected...");
    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception? exception)
{
    // Cleanup logic
    _logger.LogInformation("User disconnected...");
    await base.OnDisconnectedAsync(exception);
}
```

### Error Handling
All hubs use try-catch with logging and client notification:
```csharp
try
{
    // Hub operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    await Clients.Caller.SendAsync("OperationFailed", errorInfo);
}
```

## Client Integration Examples

### Blazor Client Setup
```csharp
// Connection with authentication
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7001/hubs/kitchen", options =>
    {
        options.AccessTokenProvider = async () => await GetAccessTokenAsync();
    })
    .WithAutomaticReconnect()
    .Build();

await connection.StartAsync();
```

### JavaScript Client Setup
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/kitchen", {
        accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect()
    .build();

await connection.start();
```

### Receiving Messages
```csharp
// C# (Blazor)
connection.On<KitchenOrderMessage>("NewOrderReceived", (message) =>
{
    // Handle new order
    StateHasChanged();
});
```

```javascript
// JavaScript
connection.on("NewOrderReceived", (message) => {
    // Handle new order
    updateKitchenDisplay(message);
});
```

### Sending Messages
```csharp
// C# (Blazor)
await connection.InvokeAsync("SendOrderToKitchen", orderMessage);
```

```javascript
// JavaScript
await connection.invoke("SendOrderToKitchen", orderMessage);
```

## Message Models

### KitchenOrderMessage
```csharp
public class KitchenOrderMessage
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public ServiceType ServiceType { get; set; }
    public byte? TableNumber { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MinutesAgo { get; set; }
    public int Priority { get; set; }
}
```

### OrderStatusChangedMessage
```csharp
public class OrderStatusChangedMessage
{
    public int OrderId { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public int ChangedBy { get; set; }
    public string ChangedByName { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### OrderLockedMessage
```csharp
public class OrderLockedMessage
{
    public int OrderId { get; set; }
    public int LockedBy { get; set; }
    public string LockedByName { get; set; }
    public DateTime LockExpiresAt { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### OrderUnlockedMessage
```csharp
public class OrderUnlockedMessage
{
    public int OrderId { get; set; }
    public int UnlockedBy { get; set; }
    public string UnlockedByName { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### ServerCommandMessage
```csharp
public class ServerCommandMessage
{
    public string CommandId { get; set; }
    public ServerCommandType CommandType { get; set; }
    public string DeviceId { get; set; }
    public int UserId { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

## SignalR Method Constants

All method names are defined in `SignalRMethods.cs`:

```csharp
public static class SignalRMethods
{
    public static class Kitchen
    {
        public const string SendOrderToKitchen = "SendOrderToKitchen";
        public const string UpdateOrderStatus = "UpdateOrderStatus";
        public const string OrderStatusChanged = "OrderStatusChanged";
        public const string NewOrderReceived = "NewOrderReceived";
        public const string JoinKitchenGroup = "JoinKitchenGroup";
        public const string LeaveKitchenGroup = "LeaveKitchenGroup";
    }
    
    public static class OrderLock
    {
        public const string AcquireLock = "AcquireLock";
        public const string ReleaseLock = "ReleaseLock";
        public const string OrderLocked = "OrderLocked";
        public const string OrderUnlocked = "OrderUnlocked";
        public const string LockExpired = "LockExpired";
    }
    
    public static class ServerCommand
    {
        public const string SendCommand = "SendCommand";
        public const string CommandReceived = "CommandReceived";
        public const string CommandCompleted = "CommandCompleted";
        public const string CommandFailed = "CommandFailed";
        public const string GetCommandStatus = "GetCommandStatus";
    }
}
```

## Known Limitations

### 1. In-Memory Command Queue (ServerCommandHub)
**Current**: Commands stored in static `ConcurrentDictionary`
**Issue**: Commands lost on server restart, not shared across multiple servers
**Solution**: Use Redis or database for persistent command queue

```csharp
// Proposed: Use Redis for distributed command queue
public class RedisCommandQueue : ICommandQueue
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task EnqueueAsync(ServerCommandMessage command)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"command:{command.CommandId}", 
            JsonSerializer.Serialize(command),
            TimeSpan.FromHours(1));
    }
}
```

### 2. No Message Persistence
**Current**: Messages are not persisted
**Issue**: Clients miss messages if disconnected
**Solution**: Implement message queue or event sourcing

### 3. Single Server Limitation
**Current**: Groups and connections are in-memory
**Issue**: Won't work with multiple servers (load balancing)
**Solution**: Add Redis backplane

```csharp
// Add to Program.cs
services.AddSignalR()
    .AddStackExchangeRedis(configuration.GetConnectionString("Redis"));
```

### 4. No Message Acknowledgment
**Current**: Fire-and-forget message delivery
**Issue**: No confirmation that clients received messages
**Solution**: Implement acknowledgment pattern

## Performance Considerations

### Scalability
- **Current**: Single server, in-memory state
- **Production**: Requires Redis backplane for multi-server
- **Connection Limits**: Monitor and configure based on server capacity

### Message Size
- **Limit**: 128 KB per message (configured)
- **Recommendation**: Use pagination for large data sets
- **Compression**: Consider enabling for large payloads

### Connection Management
- **Keep-Alive**: 15 seconds
- **Timeout**: 30 seconds
- **Reconnection**: Automatic with exponential backoff

## Security Features

### Authentication
- JWT token required for all connections
- Token validated on connection
- User context available in all hub methods

### Authorization
- Role-based access can be added via `[Authorize(Roles = "...")]`
- Method-level authorization possible
- Connection-level authorization enforced

### Logging
- All operations logged with user context
- Connection/disconnection events tracked
- Error logging with full exception details
- Audit trail for sensitive operations

## Testing Recommendations

### Unit Tests
```csharp
[TestClass]
public class KitchenHubTests
{
    [TestMethod]
    public async Task SendOrderToKitchen_BroadcastsToGroup()
    {
        // Arrange
        var hub = CreateHub();
        var message = new KitchenOrderMessage { OrderId = 1 };
        
        // Act
        await hub.SendOrderToKitchen(message);
        
        // Assert
        _mockClients.Verify(x => x.Group("Kitchen")
            .SendAsync("NewOrderReceived", message, default), 
            Times.Once);
    }
}
```

### Integration Tests
```csharp
[TestClass]
public class SignalRIntegrationTests
{
    [TestMethod]
    public async Task EndToEnd_KitchenOrderFlow()
    {
        // Arrange
        var connection = await CreateConnectionAsync("/hubs/kitchen");
        var orderReceived = false;
        
        connection.On<KitchenOrderMessage>("NewOrderReceived", (msg) =>
        {
            orderReceived = true;
        });
        
        await connection.InvokeAsync("JoinKitchenGroup");
        
        // Act
        await connection.InvokeAsync("SendOrderToKitchen", 
            new KitchenOrderMessage { OrderId = 1 });
        
        await Task.Delay(1000);
        
        // Assert
        Assert.IsTrue(orderReceived);
    }
}
```

### Manual Testing
```bash
# Using SignalR CLI tool
dotnet tool install -g Microsoft.dotnet-httprepl

# Connect to hub
httprepl https://localhost:7001
connect /hubs/kitchen

# Test methods
invoke JoinKitchenGroup
invoke SendOrderToKitchen {"orderId":1,"status":0}
```

## Build Status
✅ **Build Succeeded** with no errors
✅ **No Diagnostics Issues** in any hub

## Next Steps

### Phase 10: Backend Core Checkpoint
- Test all API endpoints
- Verify database transactions
- Test SignalR hubs with manual clients
- Performance benchmarking
- Security audit

### Future Enhancements
1. **Redis Backplane**: Enable multi-server deployment
2. **Message Persistence**: Store messages for offline clients
3. **Message Acknowledgment**: Confirm message delivery
4. **Rate Limiting**: Prevent hub method abuse
5. **Compression**: Enable for large messages
6. **Monitoring**: Add metrics and health checks

## Completion Status
✅ Phase 9 completed successfully
✅ All 3 SignalR hubs implemented
✅ Real-time communication enabled
✅ Ready for client integration
✅ Ready for Phase 10 checkpoint

---

**Completed**: March 6, 2026
**Build Status**: ✅ Success
**Hub Endpoints**:
- `/hubs/kitchen` - Kitchen display communication
- `/hubs/orderlock` - Order locking notifications
- `/hubs/servercommand` - Device command distribution
