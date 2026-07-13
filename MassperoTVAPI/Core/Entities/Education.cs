namespace MassperoTVAPI.Core.Entities;

public class Education
{
    public int Id { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string? Grade { get; set; }

    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}
