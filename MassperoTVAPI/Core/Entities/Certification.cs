namespace MassperoTVAPI.Core.Entities;

public class Certification
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }

    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}
