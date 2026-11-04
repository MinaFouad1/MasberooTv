using MassperoTVAPI.Modules.Hr.Domain.Entities.Interviews;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class InterviewType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Interview> Interviews { get; set; } = [];
}
