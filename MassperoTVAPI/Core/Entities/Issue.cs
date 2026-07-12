using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.Entities;

public class Issue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public IssueStatus Status { get; set; } = IssueStatus.Pending;

    public IssuePriority? Priority { get; set; }
    public DateTime? Date { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    // FK → Process
    public int ProcessId { get; set; }
    public Process Process { get; set; } = null!;
}
