namespace MassperoTVAPI.Core.Enums;

/// <summary>
/// Represents the priority level of an issue.
/// Stored as int in the database.
/// </summary>
public enum IssuePriority
{
    Low    = 1,
    Medium = 2,
    High   = 3,
    Critical = 4
}
