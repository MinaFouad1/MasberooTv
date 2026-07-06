namespace MassperoTVAPI.Core.Entities;

public class SecurityClearanceStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Candidate> Candidates { get; set; } = [];
}
