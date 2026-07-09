namespace MassperoTVAPI.Core.DTOs;

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record ProcessStatusDto(int Id, string Name, decimal Percentage);

public record ProcessDependencySummaryDto(int ProcessId, string ProcessName);

public record ProcessDto(
    int                               Id,
    string                            Name,
    DateTime                          StartDate,
    DateTime?                         EndDate,
    int                               PhaseId,
    string                            PhaseName,
    int                               ProcessStatusId,
    ProcessStatusDto                  ProcessStatus,
    IEnumerable<ProcessDependencySummaryDto> DependsOn,
    IEnumerable<IssueDto>             Issues
);

public record IssueDto(int Id, string Name);

public record PhaseWithProcessesDto(
    int Id,
    string Name,
    int TotalProcesses,
    decimal AverageCompletion,
    IEnumerable<ProcessDto> Processes
);

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record CreateProcessDto(
    string    Name,
    DateTime  StartDate,
    DateTime? EndDate,
    int       PhaseId
);

public record UpdateProcessDto(
    string    Name,
    DateTime  StartDate,
    DateTime? EndDate,
    int       PhaseId,
    int      statusId
);

/// <summary>
/// Payload for PATCH /api/processes/{id}/dependencies.
/// Replaces the full dependency list for the given process.
/// Pass an empty list to clear all dependencies.
/// </summary>
public record ProcessDependencyDto(IEnumerable<int> DependsOnProcessIds);

public record ChangeProcessStatusDto(int StatusId);
