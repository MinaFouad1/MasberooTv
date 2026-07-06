namespace MassperoTVAPI.Core.Entities;

public class Phase
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Process> Processes { get; set; } = [];
}
