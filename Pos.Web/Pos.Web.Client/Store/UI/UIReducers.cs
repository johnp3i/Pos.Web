using Fluxor;

namespace Pos.Web.Client.Store.UI;

/// <summary>
/// Reducers for UI state
/// </summary>
public static class UIReducers
{
    // ===== Loading Reducers =====
    
    [ReducerMethod]
    public static UIState ReduceShowLoadingAction(UIState state, UIActions.ShowLoadingAction action)
    {
        return state with
        {
            IsLoading = true,
            LoadingMessage = action.Message
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceHideLoadingAction(UIState state, UIActions.HideLoadingAction action)
    {
        return state with
        {
            IsLoading = false,
            LoadingMessage = null
        };
    }
    
    // ===== Notification Reducers =====
    
    [ReducerMethod]
    public static UIState ReduceShowNotificationAction(UIState state, UIActions.ShowNotificationAction action)
    {
        var notification = new Notification
        {
            Message = action.Message,
            Severity = action.Severity,
            Title = action.Title,
            Duration = action.Duration
        };
        
        var updatedNotifications = new List<Notification>(state.Notifications) { notification };
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceShowSuccessAction(UIState state, UIActions.ShowSuccessAction action)
    {
        var notification = new Notification
        {
            Message = action.Message,
            Severity = NotificationSeverity.Success,
            Title = action.Title ?? "Success",
            Duration = action.Duration
        };
        
        var updatedNotifications = new List<Notification>(state.Notifications) { notification };
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceShowInfoAction(UIState state, UIActions.ShowInfoAction action)
    {
        var notification = new Notification
        {
            Message = action.Message,
            Severity = NotificationSeverity.Info,
            Title = action.Title ?? "Info",
            Duration = action.Duration
        };
        
        var updatedNotifications = new List<Notification>(state.Notifications) { notification };
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceShowWarningAction(UIState state, UIActions.ShowWarningAction action)
    {
        var notification = new Notification
        {
            Message = action.Message,
            Severity = NotificationSeverity.Warning,
            Title = action.Title ?? "Warning",
            Duration = action.Duration
        };
        
        var updatedNotifications = new List<Notification>(state.Notifications) { notification };
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceShowErrorAction(UIState state, UIActions.ShowErrorAction action)
    {
        var notification = new Notification
        {
            Message = action.Message,
            Severity = NotificationSeverity.Error,
            Title = action.Title ?? "Error",
            Duration = action.Duration // 0 = no auto-dismiss for errors
        };
        
        var updatedNotifications = new List<Notification>(state.Notifications) { notification };
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceDismissNotificationAction(UIState state, UIActions.DismissNotificationAction action)
    {
        var updatedNotifications = state.Notifications.Where(n => n.Id != action.NotificationId).ToList();
        
        return state with
        {
            Notifications = updatedNotifications
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceClearAllNotificationsAction(UIState state, UIActions.ClearAllNotificationsAction action)
    {
        return state with
        {
            Notifications = new List<Notification>()
        };
    }
    
    // ===== Sidebar Reducers =====
    
    [ReducerMethod]
    public static UIState ReduceToggleSidebarAction(UIState state, UIActions.ToggleSidebarAction action)
    {
        return state with
        {
            IsSidebarOpen = !state.IsSidebarOpen
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceOpenSidebarAction(UIState state, UIActions.OpenSidebarAction action)
    {
        return state with
        {
            IsSidebarOpen = true
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceCloseSidebarAction(UIState state, UIActions.CloseSidebarAction action)
    {
        return state with
        {
            IsSidebarOpen = false
        };
    }
    
    // ===== Theme Reducers =====
    
    [ReducerMethod]
    public static UIState ReduceSetThemeAction(UIState state, UIActions.SetThemeAction action)
    {
        return state with
        {
            Theme = action.Theme
        };
    }
    
    [ReducerMethod]
    public static UIState ReduceToggleThemeAction(UIState state, UIActions.ToggleThemeAction action)
    {
        var newTheme = state.Theme == "light" ? "dark" : "light";
        
        return state with
        {
            Theme = newTheme
        };
    }
}
