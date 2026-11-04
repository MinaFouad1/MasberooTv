using MassperoTVAPI.Modules.Hr.Domain.Entities.Jobs;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class Location
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Job> Jobs { get; set; } = [];
}
