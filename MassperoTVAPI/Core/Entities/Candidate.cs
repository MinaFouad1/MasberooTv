namespace MassperoTVAPI.Core.Entities;

public class Candidate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CvFile { get; set; }
    public string? ReasonOfAccept { get; set; }
    public string? ReasonOfReject { get; set; }
    public bool? Accepted { get; set; }

    // FK → Job
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    // FK → Status (candidate application status)
    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;

    // FK → SecurityClearanceStatus
    public int SecurityClearanceId { get; set; }
    public SecurityClearanceStatus SecurityClearance { get; set; } = null!;

    // FK → ApplicationUser (nullable — assigned reviewer/HR user)
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Navigation
    public ICollection<Interview> Interviews { get; set; } = [];
}
