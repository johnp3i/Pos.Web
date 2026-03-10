using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Web.Infrastructure.Services;

namespace Pos.Web.API.Controllers;

/// <summary>
/// Reports controller for generating sales and inventory reports
/// Provides endpoints for daily sales reports, inventory reports, and report exports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get daily sales report for a specific date
    /// </summary>
    /// <param name="date">Date to generate report for (format: yyyy-MM-dd). Defaults to today if not provided.</param>
    /// <returns>Daily sales report with totals, breakdowns, and top products</returns>
    /// <response code="200">Returns the daily sales report</response>
    /// <response code="400">Invalid date parameter</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("daily-sales")]
    [ProducesResponseType(typeof(DailySalesReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDailySalesReport([FromQuery] DateTime? date = null)
    {
        try
        {
            var reportDate = date ?? DateTime.Today;

            // Validate date is not in the future
            if (reportDate.Date > DateTime.Today)
            {
                return BadRequest(new { message = "Report date cannot be in the future" });
            }

            // Validate date is not too far in the past (e.g., more than 2 years)
            if (reportDate.Date < DateTime.Today.AddYears(-2))
            {
                return BadRequest(new { message = "Report date cannot be more than 2 years in the past" });
            }

            _logger.LogInformation("Generating daily sales report for {Date}", reportDate);

            var report = await _reportService.GetDailySalesReportAsync(reportDate);

            _logger.LogInformation(
                "Daily sales report generated successfully for {Date}: {TotalSales} in sales, {TotalOrders} orders",
                reportDate, report.TotalSales, report.TotalOrders);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales report");
            return StatusCode(500, new { message = "An error occurred while generating the sales report" });
        }
    }

    /// <summary>
    /// Get inventory report showing current stock levels
    /// </summary>
    /// <param name="lowStockThreshold">Threshold for low stock alert (default: 10)</param>
    /// <param name="categoryId">Filter by category ID (optional)</param>
    /// <returns>Inventory report with stock levels and alerts</returns>
    /// <response code="200">Returns the inventory report</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("inventory")]
    [ProducesResponseType(typeof(InventoryReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventoryReport(
        [FromQuery] int? lowStockThreshold = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            // Validate threshold
            if (lowStockThreshold.HasValue && (lowStockThreshold.Value < 0 || lowStockThreshold.Value > 1000))
            {
                return BadRequest(new { message = "Low stock threshold must be between 0 and 1000" });
            }

            // Validate category ID
            if (categoryId.HasValue && categoryId.Value <= 0)
            {
                return BadRequest(new { message = "Category ID must be greater than 0" });
            }

            _logger.LogInformation(
                "Generating inventory report with threshold {Threshold}, category {CategoryId}",
                lowStockThreshold, categoryId);

            var report = await _reportService.GetInventoryReportAsync(lowStockThreshold, categoryId);

            _logger.LogInformation(
                "Inventory report generated successfully: {TotalProducts} products, {LowStock} low stock, {OutOfStock} out of stock",
                report.TotalProducts, report.LowStockProducts, report.OutOfStockProducts);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory report");
            return StatusCode(500, new { message = "An error occurred while generating the inventory report" });
        }
    }

    /// <summary>
    /// Export report to PDF or Excel format
    /// </summary>
    /// <param name="request">Export request containing report type, format, and parameters</param>
    /// <returns>File download with the exported report</returns>
    /// <response code="200">Returns the exported file</response>
    /// <response code="400">Invalid export parameters</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Export request is required" });
            }

            if (!Enum.IsDefined(typeof(ReportType), request.ReportType))
            {
                return BadRequest(new { message = "Invalid report type" });
            }

            if (!Enum.IsDefined(typeof(ExportFormat), request.Format))
            {
                return BadRequest(new { message = "Invalid export format" });
            }

            _logger.LogInformation(
                "Exporting {ReportType} report as {Format}",
                request.ReportType, request.Format);

            var result = await _reportService.ExportReportAsync(
                request.ReportType,
                request.Format,
                request.Parameters ?? new Dictionary<string, object>());

            _logger.LogInformation(
                "Report exported successfully: {FileName}, {FileSize} bytes",
                result.FileName, result.FileContent.Length);

            return File(result.FileContent, result.ContentType, result.FileName);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Unsupported report type or format");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            return StatusCode(500, new { message = "An error occurred while exporting the report" });
        }
    }

    /// <summary>
    /// Get available report types
    /// </summary>
    /// <returns>List of available report types</returns>
    /// <response code="200">Returns list of report types</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<ReportTypeInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetReportTypes()
    {
        var reportTypes = new List<ReportTypeInfo>
        {
            new ReportTypeInfo
            {
                Type = ReportType.DailySales,
                Name = "Daily Sales Report",
                Description = "Comprehensive sales report for a specific date including totals, breakdowns, and top products",
                Parameters = new List<string> { "date" }
            },
            new ReportTypeInfo
            {
                Type = ReportType.Inventory,
                Name = "Inventory Report",
                Description = "Current stock levels with low stock and out of stock alerts",
                Parameters = new List<string> { "lowStockThreshold", "categoryId" }
            }
        };

        return Ok(reportTypes);
    }

    /// <summary>
    /// Get sales summary for a date range
    /// </summary>
    /// <param name="fromDate">Start date (format: yyyy-MM-dd)</param>
    /// <param name="toDate">End date (format: yyyy-MM-dd)</param>
    /// <returns>Sales summary for the date range</returns>
    /// <response code="200">Returns sales summary</response>
    /// <response code="400">Invalid date range</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("sales-summary")]
    [ProducesResponseType(typeof(SalesSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return BadRequest(new { message = "fromDate cannot be greater than toDate" });
            }

            if (toDate > DateTime.Today)
            {
                return BadRequest(new { message = "toDate cannot be in the future" });
            }

            var daysDiff = (toDate - fromDate).Days;
            if (daysDiff > 365)
            {
                return BadRequest(new { message = "Date range cannot exceed 365 days" });
            }

            _logger.LogInformation(
                "Generating sales summary from {FromDate} to {ToDate}",
                fromDate, toDate);

            // Generate summary by aggregating daily reports
            var summary = new SalesSummaryDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalDays = daysDiff + 1
            };

            // Aggregate data for each day in the range
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var dailyReport = await _reportService.GetDailySalesReportAsync(date);
                summary.TotalSales += dailyReport.TotalSales;
                summary.TotalOrders += dailyReport.TotalOrders;
                summary.TotalTax += dailyReport.TotalTax;
                summary.TotalDiscounts += dailyReport.TotalDiscounts;
            }

            summary.AverageDailySales = summary.TotalDays > 0 ? summary.TotalSales / summary.TotalDays : 0;
            summary.AverageOrderValue = summary.TotalOrders > 0 ? summary.TotalSales / summary.TotalOrders : 0;

            _logger.LogInformation(
                "Sales summary generated: {TotalSales} in sales over {TotalDays} days",
                summary.TotalSales, summary.TotalDays);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales summary");
            return StatusCode(500, new { message = "An error occurred while generating the sales summary" });
        }
    }
}

/// <summary>
/// Export report request model
/// </summary>
public class ExportReportRequest
{
    /// <summary>
    /// Type of report to export
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// Export format (PDF or Excel)
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// Report parameters (e.g., date, threshold, categoryId)
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Report type information
/// </summary>
public class ReportTypeInfo
{
    public ReportType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
}

/// <summary>
/// Sales summary DTO for date range
/// </summary>
public class SalesSummaryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageDailySales { get; set; }
    public decimal AverageOrderValue { get; set; }
}
