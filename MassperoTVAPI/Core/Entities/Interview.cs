namespace MassperoTVAPI.Core.Entities;

public class Interview
{
    public int Id { get; set; }
    public string? Grade { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? InterviewMode { get; set; }
    public DateTime? InterviewDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    // Optional FK → ApplicationUser who submitted the grade.
    public string? EvaluatorId { get; set; }
    public ApplicationUser? Evaluator { get; set; }

    // FK → Candidate
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    // FK → InterviewType
    public int TypeId { get; set; }
    public InterviewType Type { get; set; } = null!;
}
