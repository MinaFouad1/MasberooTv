using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];
}
