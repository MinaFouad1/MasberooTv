namespace MassperoTVAPI.Core.Entities;

public class Issue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool? Resolved { get; set; } = false;

    public string? Priority { get; set; }
    public DateTime? Date { get; set; } = DateTime.UtcNow;

    // FK → Process
    public int ProcessId { get; set; }
    public Process Process { get; set; } = null!;
}
