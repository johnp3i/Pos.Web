namespace Pos.Web.Shared.Enums;

/// <summary>
/// Represents the type of server command for device-to-master communication
/// </summary>
public enum ServerCommandType
{
    /// <summary>
    /// Print receipt command
    /// </summary>
    PrintReceipt = 1,
    
    /// <summary>
    /// Print kitchen order command
    /// </summary>
    PrintKitchenOrder = 2,
    
    /// <summary>
    /// Open cash drawer command
    /// </summary>
    OpenCashDrawer = 3,
    
    /// <summary>
    /// Print label command
    /// </summary>
    PrintLabel = 4,
    
    /// <summary>
    /// Sync data command
    /// </summary>
    SyncData = 5,
    
    /// <summary>
    /// Refresh configuration command
    /// </summary>
    RefreshConfig = 6
}
