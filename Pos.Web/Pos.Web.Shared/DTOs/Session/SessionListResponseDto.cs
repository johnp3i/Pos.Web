namespace Pos.Web.Shared.DTOs.Session;

/// <summary>
/// Response DTO containing a list of user sessions.
/// </summary>
public class SessionListResponseDto
{
    /// <summary>
    /// List of user sessions
    /// </summary>
    public List<UserSessionDto> Sessions { get; set; } = new();

    /// <summary>
    /// Total count of sessions
    /// </summary>
    public int TotalCount { get; set; }
}
