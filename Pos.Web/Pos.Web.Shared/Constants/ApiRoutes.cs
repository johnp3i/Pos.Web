namespace Pos.Web.Shared.Constants;

/// <summary>
/// API route constants
/// </summary>
public static class ApiRoutes
{
    public const string BaseUrl = "/api";
    
    /// <summary>
    /// Authentication routes
    /// </summary>
    public static class Auth
    {
        public const string Base = $"{BaseUrl}/auth";
        public const string Login = $"{Base}/login";
        public const string Refresh = $"{Base}/refresh";
        public const string Logout = $"{Base}/logout";
    }
    
    /// <summary>
    /// Order routes
    /// </summary>
    public static class Orders
    {
        public const string Base = $"{BaseUrl}/orders";
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string GetPending = $"{Base}/pending";
        public const string Split = $"{Base}/{{id}}/split";
    }
    
    /// <summary>
    /// Payment routes
    /// </summary>
    public static class Payments
    {
        public const string Base = $"{BaseUrl}/payments";
        public const string Process = Base;
        public const string ApplyDiscount = $"{Base}/discount";
        public const string SplitPayment = $"{Base}/split";
    }
    
    /// <summary>
    /// Customer routes
    /// </summary>
    public static class Customers
    {
        public const string Base = $"{BaseUrl}/customers";
        public const string Search = $"{Base}/search";
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string GetHistory = $"{Base}/{{id}}/history";
    }
    
    /// <summary>
    /// Product routes
    /// </summary>
    public static class Products
    {
        public const string Base = $"{BaseUrl}/products";
        public const string GetAll = Base;
        public const string Search = $"{Base}/search";
        public const string GetCategories = $"{Base}/categories";
        public const string GetByCategory = $"{Base}/category/{{id}}";
    }
    
    /// <summary>
    /// Kitchen routes
    /// </summary>
    public static class Kitchen
    {
        public const string Base = $"{BaseUrl}/kitchen";
        public const string GetOrders = $"{Base}/orders";
        public const string UpdateStatus = $"{Base}/orders/{{id}}/status";
        public const string GetHistory = $"{Base}/orders/history";
    }
    
    /// <summary>
    /// Report routes
    /// </summary>
    public static class Reports
    {
        public const string Base = $"{BaseUrl}/reports";
        public const string DailySales = $"{Base}/daily-sales";
        public const string Inventory = $"{Base}/inventory";
        public const string Export = $"{Base}/export";
    }
}

/// <summary>
/// SignalR hub routes
/// </summary>
public static class Hubs
{
    public const string Kitchen = "/hubs/kitchen";
    public const string OrderLock = "/hubs/orderlock";
    public const string ServerCommand = "/hubs/servercommand";
}