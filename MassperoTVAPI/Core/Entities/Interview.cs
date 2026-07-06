namespace MassperoTVAPI.Core.Entities;

public class Interview
{
    public int Id { get; set; }
    public string? Grade { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // FK → Candidate
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    // FK → InterviewType
    public int TypeId { get; set; }
    public InterviewType Type { get; set; } = null!;
}
