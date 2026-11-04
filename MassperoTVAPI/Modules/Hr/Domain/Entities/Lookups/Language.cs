using MassperoTVAPI.Modules.Hr.Domain.Entities.Candidates;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class Language
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CandidateLanguage> CandidateLanguages { get; set; } = [];
}
