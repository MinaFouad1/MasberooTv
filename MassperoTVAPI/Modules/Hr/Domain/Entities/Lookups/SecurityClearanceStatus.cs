using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class SecurityClearanceStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Candidate> Candidates { get; set; } = [];
}
