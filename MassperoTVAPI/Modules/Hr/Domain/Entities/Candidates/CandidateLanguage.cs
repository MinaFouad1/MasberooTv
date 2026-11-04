using MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

public class CandidateLanguage
{
    public int Id { get; set; }

    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}
