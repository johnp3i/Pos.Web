namespace Pos.Web.Client.Services.SignalR
{
    /// <summary>
    /// Implementation of kitchen hub service
    /// </summary>
    public class KitchenHubService : IKitchenHubService
    {
        private readonly ISignalRService _signalRService;

        public KitchenHubService(ISignalRService signalRService)
        {
            _signalRService = signalRService;
        }

        /// <summary>
        /// Subscribes to kitchen order updates
        /// </summary>
        public IDisposable SubscribeToOrderUpdates(Action<int, string> onOrderStatusChanged)
        {
            return _signalRService.On<int, string>("OrderStatusChanged", onOrderStatusChanged);
        }

        /// <summary>
        /// Subscribes to new order notifications
        /// </summary>
        public IDisposable SubscribeToNewOrders(Action<int> onNewOrder)
        {
            return _signalRService.On<int>("NewOrderReceived", onNewOrder);
        }

        /// <summary>
        /// Updates an order status
        /// </summary>
        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            await _signalRService.InvokeAsync("UpdateOrderStatus", orderId, status);
        }

        /// <summary>
        /// Joins the kitchen group for receiving updates
        /// </summary>
        public async Task JoinKitchenGroupAsync()
        {
            await _signalRService.InvokeAsync("JoinKitchenGroup");
        }

        /// <summary>
        /// Leaves the kitchen group
        /// </summary>
        public async Task LeaveKitchenGroupAsync()
        {
            await _signalRService.InvokeAsync("LeaveKitchenGroup");
        }
    }
}
