namespace Pos.Web.Client.Store.UI;

/// <summary>
/// Actions for UI state management
/// </summary>
public static class UIActions
{
    // ===== Loading Actions =====
    
    /// <summary>
    /// Show global loading indicator
    /// </summary>
    public record ShowLoadingAction(string? Message = null);
    
    /// <summary>
    /// Hide global loading indicator
    /// </summary>
    public record HideLoadingAction();
    
    // ===== Notification Actions =====
    
    /// <summary>
    /// Show a notification
    /// </summary>
    public record ShowNotificationAction(
        string Message,
        NotificationSeverity Severity = NotificationSeverity.Info,
        string? Title = null,
        int Duration = 5000);
    
    /// <summary>
    /// Show a success notification
    /// </summary>
    public record ShowSuccessAction(string Message, string? Title = null, int Duration = 5000);
    
    /// <summary>
    /// Show an info notification
    /// </summary>
    public record ShowInfoAction(string Message, string? Title = null, int Duration = 5000);
    
    /// <summary>
    /// Show a warning notification
    /// </summary>
    public record ShowWarningAction(string Message, string? Title = null, int Duration = 5000);
    
    /// <summary>
    /// Show an error notification
    /// </summary>
    public record ShowErrorAction(string Message, string? Title = null, int Duration = 0);
    
    /// <summary>
    /// Dismiss a notification
    /// </summary>
    public record DismissNotificationAction(string NotificationId);
    
    /// <summary>
    /// Clear all notifications
    /// </summary>
    public record ClearAllNotificationsAction();
    
    // ===== Sidebar Actions =====
    
    /// <summary>
    /// Toggle sidebar open/closed
    /// </summary>
    public record ToggleSidebarAction();
    
    /// <summary>
    /// Open sidebar
    /// </summary>
    public record OpenSidebarAction();
    
    /// <summary>
    /// Close sidebar
    /// </summary>
    public record CloseSidebarAction();
    
    // ===== Theme Actions =====
    
    /// <summary>
    /// Set theme (light/dark)
    /// </summary>
    public record SetThemeAction(string Theme);
    
    /// <summary>
    /// Toggle theme between light and dark
    /// </summary>
    public record ToggleThemeAction();
}
