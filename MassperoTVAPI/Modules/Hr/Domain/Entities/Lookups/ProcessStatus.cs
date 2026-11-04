using MassperoTVAPI.Modules.Hr.Domain.Entities.Processes;

namespace MassperoTVAPI.Modules.Hr.Domain.Entities.Lookups;

public class ProcessStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }

    // Navigation
    public ICollection<Process> Processes { get; set; } = [];
}
