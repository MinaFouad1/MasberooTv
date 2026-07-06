namespace MassperoTVAPI.Core.Entities;

/// <summary>
/// Junction table representing the dependency relationship between processes.
/// A Process can depend on many other Processes (and vice-versa).
/// </summary>
public class ProcessDependency
{
    /// <summary>The process that has the dependency.</summary>
    public int ProcessId { get; set; }
    public Process Process { get; set; } = null!;

    /// <summary>The process that must be completed first.</summary>
    public int DependsOnProcessId { get; set; }
    public Process DependsOn { get; set; } = null!;
}
