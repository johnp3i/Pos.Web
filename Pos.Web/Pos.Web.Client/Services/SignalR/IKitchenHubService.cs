namespace Pos.Web.Client.Services.SignalR
{
    /// <summary>
    /// Service for kitchen-related SignalR operations
    /// </summary>
    public interface IKitchenHubService
    {
        /// <summary>
        /// Subscribes to kitchen order updates
        /// </summary>
        IDisposable SubscribeToOrderUpdates(Action<int, string> onOrderStatusChanged);

        /// <summary>
        /// Subscribes to new order notifications
        /// </summary>
        IDisposable SubscribeToNewOrders(Action<int> onNewOrder);

        /// <summary>
        /// Updates an order status
        /// </summary>
        Task UpdateOrderStatusAsync(int orderId, string status);

        /// <summary>
        /// Joins the kitchen group for receiving updates
        /// </summary>
        Task JoinKitchenGroupAsync();

        /// <summary>
        /// Leaves the kitchen group
        /// </summary>
        Task LeaveKitchenGroupAsync();
    }
}
