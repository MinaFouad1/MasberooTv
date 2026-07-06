namespace MassperoTVAPI.Core.Entities;

public class Process
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // FK → Phase
    public int PhaseId { get; set; }
    public Phase Phase { get; set; } = null!;

    // FK → ProcessStatus
    public int ProcessStatusId { get; set; }
    public ProcessStatus ProcessStatus { get; set; } = null!;

    // Self-join many-to-many via ProcessDependency junction table
    // Processes that THIS process depends on
    public ICollection<ProcessDependency> Dependencies { get; set; } = [];
    // Processes that depend on THIS process
    public ICollection<ProcessDependency> DependentProcesses { get; set; } = [];

    // Navigation
    public ICollection<Issue> Issues { get; set; } = [];
}
