using Pos.Web.Shared.DTOs;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Report service interface for generating sales and inventory reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Get daily sales report for a specific date
    /// </summary>
    /// <param name="date">Date to generate report for</param>
    /// <returns>Daily sales report with totals and breakdown</returns>
    Task<DailySalesReportDto> GetDailySalesReportAsync(DateTime date);

    /// <summary>
    /// Get inventory report showing current stock levels
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for low stock alert (optional)</param>
    /// <param name="categoryId">Filter by category (optional)</param>
    /// <returns>Inventory report with stock levels and alerts</returns>
    Task<InventoryReportDto> GetInventoryReportAsync(int? lowStockThreshold = null, int? categoryId = null);

    /// <summary>
    /// Export report to specified format
    /// </summary>
    /// <param name="reportType">Type of report to export</param>
    /// <param name="format">Export format (PDF or Excel)</param>
    /// <param name="parameters">Report parameters</param>
    /// <returns>Byte array of exported file</returns>
    Task<ReportExportResult> ExportReportAsync(ReportType reportType, ExportFormat format, Dictionary<string, object> parameters);
}

/// <summary>
/// Daily sales report DTO
/// </summary>
public class DailySalesReportDto
{
    public DateTime ReportDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal NetSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<SalesByCategoryDto> SalesByCategory { get; set; } = new();
    public List<SalesByHourDto> SalesByHour { get; set; } = new();
    public List<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
}

/// <summary>
/// Sales by category breakdown
/// </summary>
public class SalesByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Sales by hour breakdown
/// </summary>
public class SalesByHourDto
{
    public int Hour { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Payment method breakdown
/// </summary>
public class PaymentMethodBreakdownDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Top selling product
/// </summary>
public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Inventory report DTO
/// </summary>
public class InventoryReportDto
{
    public DateTime ReportDate { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<InventoryItemDto> Items { get; set; } = new();
}

/// <summary>
/// Inventory item DTO
/// </summary>
public class InventoryItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int? MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public StockStatus Status { get; set; }
}

/// <summary>
/// Stock status enum
/// </summary>
public enum StockStatus
{
    InStock,
    LowStock,
    OutOfStock
}

/// <summary>
/// Report export result
/// </summary>
public class ReportExportResult
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// Report type enum
/// </summary>
public enum ReportType
{
    DailySales,
    Inventory,
    CustomerHistory,
    ProductSales
}

/// <summary>
/// Export format enum
/// </summary>
public enum ExportFormat
{
    PDF,
    Excel
}
