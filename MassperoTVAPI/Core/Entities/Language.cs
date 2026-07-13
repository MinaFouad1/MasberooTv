namespace MassperoTVAPI.Core.Entities;

public class Language
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CandidateLanguage> CandidateLanguages { get; set; } = [];
}
