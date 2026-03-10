namespace Pos.Web.Shared.DTOs.Audit;

/// <summary>
/// Response model for audit log queries with pagination information
/// </summary>
public class AuditLogQueryResponse
{
    /// <summary>
    /// List of audit log entries
    /// </summary>
    public List<AuthAuditLogDto> Logs { get; set; } = new();

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of records per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there are more pages available
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page available
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
