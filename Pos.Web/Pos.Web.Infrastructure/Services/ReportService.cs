using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.UnitOfWork;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Report service implementation for generating sales and inventory reports
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PosDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IUnitOfWork unitOfWork,
        PosDbContext context,
        ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get daily sales report for a specific date
    /// </summary>
    public async Task<DailySalesReportDto> GetDailySalesReportAsync(DateTime date)
    {
        try
        {
            _logger.LogInformation("Generating daily sales report for {Date}", date.Date);

            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            // Get orders for the specified date
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .ToListAsync();

            var totalSales = orders.Sum(o => o.TotalAmount);
            var totalTax = orders.Sum(o => o.TaxAmount);
            var totalDiscounts = orders.Sum(o => o.DiscountAmount ?? 0);

            // Sales by category
            var salesByCategory = orders
                .SelectMany(o => o.Items)
                .Where(oi => oi.Product != null && oi.Product.Category != null)
                .GroupBy(oi => new { oi.Product!.CategoryID, oi.Product.Category!.Name })
                .Select(g => new SalesByCategoryDto
                {
                    CategoryId = g.Key.CategoryID,
                    CategoryName = g.Key.Name,
                    TotalSales = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    OrderCount = g.Select(oi => oi.InvoiceID).Distinct().Count(),
                    Percentage = totalSales > 0 ? (g.Sum(oi => oi.Quantity * oi.UnitPrice) / totalSales) * 100 : 0
                })
                .OrderByDescending(s => s.TotalSales)
                .ToList();

            // Sales by hour
            var salesByHour = orders
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new SalesByHourDto
                {
                    Hour = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(s => s.Hour)
                .ToList();

            // Top selling products
            var topSellingProducts = orders
                .SelectMany(o => o.Items)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => new { oi.CategoryItemID, oi.Product!.Name })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.CategoryItemID,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToList();

            var report = new DailySalesReportDto
            {
                ReportDate = date.Date,
                TotalSales = totalSales,
                TotalTax = totalTax,
                TotalDiscounts = totalDiscounts,
                NetSales = totalSales - totalDiscounts,
                TotalOrders = orders.Count,
                TotalCustomers = orders.Where(o => o.CustomerID.HasValue).Select(o => o.CustomerID).Distinct().Count(),
                AverageOrderValue = orders.Count > 0 ? totalSales / orders.Count : 0,
                SalesByCategory = salesByCategory,
                SalesByHour = salesByHour,
                TopSellingProducts = topSellingProducts,
                PaymentMethodBreakdown = new List<PaymentMethodBreakdownDto>() // TODO: Implement when payment data is available
            };

            _logger.LogInformation(
                "Daily sales report generated: {TotalSales} sales, {TotalOrders} orders",
                report.TotalSales, report.TotalOrders);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales report for {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Get inventory report showing current stock levels
    /// </summary>
    public async Task<InventoryReportDto> GetInventoryReportAsync(int? lowStockThreshold = null, int? categoryId = null)
    {
        try
        {
            _logger.LogInformation(
                "Generating inventory report with threshold {Threshold}, category {CategoryId}",
                lowStockThreshold, categoryId);

            var threshold = lowStockThreshold ?? 10; // Default threshold

            // Build query
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            var products = await query.ToListAsync();

            var items = products.Select(p => new InventoryItemDto
            {
                ProductId = p.ID,
                ProductName = p.Name,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                CurrentStock = p.StockQuantity,
                MinimumStock = 10, // Default minimum stock level
                UnitPrice = p.Price,
                TotalValue = p.StockQuantity * p.Price,
                Status = DetermineStockStatus(p.StockQuantity, threshold)
            }).ToList();

            var report = new InventoryReportDto
            {
                ReportDate = DateTime.Now,
                TotalProducts = items.Count,
                LowStockProducts = items.Count(i => i.Status == StockStatus.LowStock),
                OutOfStockProducts = items.Count(i => i.Status == StockStatus.OutOfStock),
                TotalInventoryValue = items.Sum(i => i.TotalValue),
                Items = items.OrderBy(i => i.Status).ThenBy(i => i.ProductName).ToList()
            };

            _logger.LogInformation(
                "Inventory report generated: {TotalProducts} products, {LowStock} low stock, {OutOfStock} out of stock",
                report.TotalProducts, report.LowStockProducts, report.OutOfStockProducts);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory report");
            throw;
        }
    }

    /// <summary>
    /// Export report to specified format
    /// </summary>
    public async Task<ReportExportResult> ExportReportAsync(
        ReportType reportType,
        ExportFormat format,
        Dictionary<string, object> parameters)
    {
        try
        {
            _logger.LogInformation("Exporting {ReportType} report as {Format}", reportType, format);

            // Generate report data based on type
            byte[] fileContent;
            string fileName;
            string contentType;

            switch (reportType)
            {
                case ReportType.DailySales:
                    var date = parameters.ContainsKey("date") ? (DateTime)parameters["date"] : DateTime.Today;
                    var salesReport = await GetDailySalesReportAsync(date);
                    fileContent = await ExportDailySalesReport(salesReport, format);
                    fileName = $"DailySalesReport_{date:yyyyMMdd}.{GetFileExtension(format)}";
                    break;

                case ReportType.Inventory:
                    var threshold = parameters.ContainsKey("threshold") ? (int?)parameters["threshold"] : null;
                    var categoryId = parameters.ContainsKey("categoryId") ? (int?)parameters["categoryId"] : null;
                    var inventoryReport = await GetInventoryReportAsync(threshold, categoryId);
                    fileContent = await ExportInventoryReport(inventoryReport, format);
                    fileName = $"InventoryReport_{DateTime.Now:yyyyMMdd}.{GetFileExtension(format)}";
                    break;

                default:
                    throw new NotSupportedException($"Report type {reportType} is not supported");
            }

            contentType = format == ExportFormat.PDF
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            _logger.LogInformation("Report exported successfully: {FileName}", fileName);

            return new ReportExportResult
            {
                FileContent = fileContent,
                FileName = fileName,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting {ReportType} report as {Format}", reportType, format);
            throw;
        }
    }

    #region Private Helper Methods

    private StockStatus DetermineStockStatus(int currentStock, int minimumStock)
    {
        if (currentStock == 0)
            return StockStatus.OutOfStock;
        if (currentStock <= minimumStock)
            return StockStatus.LowStock;
        return StockStatus.InStock;
    }

    private string GetFileExtension(ExportFormat format)
    {
        return format == ExportFormat.PDF ? "pdf" : "xlsx";
    }

    private async Task<byte[]> ExportDailySalesReport(DailySalesReportDto report, ExportFormat format)
    {
        // TODO: Implement actual PDF/Excel generation using a library like iTextSharp or EPPlus
        // For now, return a simple CSV-like format as placeholder
        await Task.CompletedTask;

        var content = $"Daily Sales Report - {report.ReportDate:yyyy-MM-dd}\n\n";
        content += $"Total Sales: ${report.TotalSales:N2}\n";
        content += $"Total Orders: {report.TotalOrders}\n";
        content += $"Average Order Value: ${report.AverageOrderValue:N2}\n\n";
        content += "Top Selling Products:\n";
        foreach (var product in report.TopSellingProducts)
        {
            content += $"- {product.ProductName}: {product.QuantitySold} units, ${product.TotalRevenue:N2}\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private async Task<byte[]> ExportInventoryReport(InventoryReportDto report, ExportFormat format)
    {
        // TODO: Implement actual PDF/Excel generation
        await Task.CompletedTask;

        var content = $"Inventory Report - {report.ReportDate:yyyy-MM-dd}\n\n";
        content += $"Total Products: {report.TotalProducts}\n";
        content += $"Low Stock: {report.LowStockProducts}\n";
        content += $"Out of Stock: {report.OutOfStockProducts}\n";
        content += $"Total Value: ${report.TotalInventoryValue:N2}\n\n";
        content += "Items:\n";
        foreach (var item in report.Items.Take(20))
        {
            content += $"- {item.ProductName}: {item.CurrentStock} units, ${item.TotalValue:N2} ({item.Status})\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    #endregion
}
