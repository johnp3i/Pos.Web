namespace Pos.Web.Shared.Constants;

/// <summary>
/// SignalR hub method name constants
/// </summary>
public static class SignalRMethods
{
    /// <summary>
    /// Kitchen hub methods
    /// </summary>
    public static class Kitchen
    {
        public const string SendOrderToKitchen = "SendOrderToKitchen";
        public const string UpdateOrderStatus = "UpdateOrderStatus";
        public const string OrderStatusChanged = "OrderStatusChanged";
        public const string NewOrderReceived = "NewOrderReceived";
        public const string JoinKitchenGroup = "JoinKitchenGroup";
        public const string LeaveKitchenGroup = "LeaveKitchenGroup";
    }
    
    /// <summary>
    /// Order lock hub methods
    /// </summary>
    public static class OrderLock
    {
        public const string AcquireLock = "AcquireLock";
        public const string ReleaseLock = "ReleaseLock";
        public const string OrderLocked = "OrderLocked";
        public const string OrderUnlocked = "OrderUnlocked";
        public const string LockExpired = "LockExpired";
    }
    
    /// <summary>
    /// Server command hub methods
    /// </summary>
    public static class ServerCommand
    {
        public const string SendCommand = "SendCommand";
        public const string CommandReceived = "CommandReceived";
        public const string CommandCompleted = "CommandCompleted";
        public const string CommandFailed = "CommandFailed";
        public const string GetCommandStatus = "GetCommandStatus";
    }
}
