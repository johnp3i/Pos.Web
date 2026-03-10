using System.ComponentModel.DataAnnotations;

namespace Pos.Web.Shared.Models;

/// <summary>
/// Request model for searching customers
/// </summary>
public class SearchCustomerRequest
{
    /// <summary>
    /// Search query (name, telephone, email)
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "Search query must be at least 2 characters")]
    [MaxLength(100)]
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    [Range(1, 100)]
    public int MaxResults { get; set; } = 20;
    
    /// <summary>
    /// Whether to include inactive customers
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}
