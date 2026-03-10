using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// Sync queue entity to queue offline operations for synchronization
/// Mapped to web.SyncQueue table
/// </summary>
[Table("SyncQueue", Schema = "web")]
public class SyncQueue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    [MaxLength(100)]
    public string DeviceID { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string OperationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityID { get; set; }

    [Required]
    public string Payload { get; set; } = string.Empty;

    [Required]
    public DateTime ClientTimestamp { get; set; }

    [Required]
    public DateTime ServerTimestamp { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    public int AttemptCount { get; set; }

    public DateTime? LastAttemptAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }

    /// <summary>
    /// Checks if the operation is pending
    /// </summary>
    [NotMapped]
    public bool IsPending => Status == "Pending";

    /// <summary>
    /// Checks if the operation is processing
    /// </summary>
    [NotMapped]
    public bool IsProcessing => Status == "Processing";

    /// <summary>
    /// Checks if the operation is completed
    /// </summary>
    [NotMapped]
    public bool IsCompleted => Status == "Completed";

    /// <summary>
    /// Checks if the operation has failed
    /// </summary>
    [NotMapped]
    public bool IsFailed => Status == "Failed";

    /// <summary>
    /// Gets the processing duration if completed
    /// </summary>
    [NotMapped]
    public TimeSpan? ProcessingDuration => ProcessedAt.HasValue 
        ? ProcessedAt.Value - ServerTimestamp 
        : null;

    /// <summary>
    /// Gets the time since last attempt
    /// </summary>
    [NotMapped]
    public TimeSpan? TimeSinceLastAttempt => LastAttemptAt.HasValue 
        ? DateTime.UtcNow - LastAttemptAt.Value 
        : null;
}
