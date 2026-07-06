namespace MassperoTVAPI.Core.Entities;

public class Status
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Candidate> Candidates { get; set; } = [];
}
