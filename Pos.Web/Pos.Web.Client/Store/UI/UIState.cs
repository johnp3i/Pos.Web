using Fluxor;

namespace Pos.Web.Client.Store.UI;

/// <summary>
/// State for UI management (loading indicators, notifications, etc.)
/// </summary>
[FeatureState]
public record UIState
{
    /// <summary>
    /// Whether a global loading indicator should be shown
    /// </summary>
    public bool IsLoading { get; init; }
    
    /// <summary>
    /// Loading message to display
    /// </summary>
    public string? LoadingMessage { get; init; }
    
    /// <summary>
    /// Active notifications
    /// </summary>
    public List<Notification> Notifications { get; init; } = new();
    
    /// <summary>
    /// Whether the sidebar is open (for mobile/tablet)
    /// </summary>
    public bool IsSidebarOpen { get; init; }
    
    /// <summary>
    /// Current theme (light/dark)
    /// </summary>
    public string Theme { get; init; } = "light";
}

/// <summary>
/// Notification model
/// </summary>
public record Notification
{
    /// <summary>
    /// Unique notification ID
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Notification message
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Notification severity (success, info, warning, error)
    /// </summary>
    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;
    
    /// <summary>
    /// Notification title (optional)
    /// </summary>
    public string? Title { get; init; }
    
    /// <summary>
    /// Timestamp when notification was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    
    /// <summary>
    /// Duration in milliseconds (0 = no auto-dismiss)
    /// </summary>
    public int Duration { get; init; } = 5000;
}

/// <summary>
/// Notification severity levels
/// </summary>
public enum NotificationSeverity
{
    Success,
    Info,
    Warning,
    Error
}
