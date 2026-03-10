using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// API audit log entity to track all API operations
/// Mapped to web.ApiAuditLog table
/// </summary>
[Table("ApiAuditLog", Schema = "web")]
public class ApiAuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public int? UserID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityType { get; set; }

    public int? EntityID { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(500)]
    public string? RequestPath { get; set; }

    [MaxLength(10)]
    public string? RequestMethod { get; set; }

    public int? StatusCode { get; set; }

    /// <summary>
    /// Request duration in milliseconds
    /// </summary>
    public int? Duration { get; set; }

    public string? ErrorMessage { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }

    /// <summary>
    /// Indicates if the operation was successful (2xx status code)
    /// </summary>
    [NotMapped]
    public bool IsSuccessful => StatusCode.HasValue && StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// Indicates if the operation resulted in an error (4xx or 5xx status code)
    /// </summary>
    [NotMapped]
    public bool IsError => StatusCode.HasValue && StatusCode >= 400;
}
