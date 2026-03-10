using Pos.Web.Shared.Enums;

namespace Pos.Web.Shared.DTOs.Audit;

/// <summary>
/// Request model for querying audit logs with filters
/// </summary>
public class AuditLogQueryRequest
{
    /// <summary>
    /// User ID to filter by (optional)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Event type to filter by (optional)
    /// </summary>
    public AuditEventType? EventType { get; set; }

    /// <summary>
    /// Start date for the query (optional)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for the query (optional)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Maximum number of records to return (default: 100, max: 1000)
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int Page { get; set; } = 1;
}
