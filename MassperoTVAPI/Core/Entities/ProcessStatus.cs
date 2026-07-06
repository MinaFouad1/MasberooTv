namespace MassperoTVAPI.Core.Entities;

public class ProcessStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }

    // Navigation
    public ICollection<Process> Processes { get; set; } = [];
}
