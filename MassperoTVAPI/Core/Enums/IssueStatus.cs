namespace MassperoTVAPI.Core.Enums;

/// <summary>
/// Represents the lifecycle status of an issue.
/// Stored as int in the database.
/// </summary>
public enum IssueStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Solved = 4
}
