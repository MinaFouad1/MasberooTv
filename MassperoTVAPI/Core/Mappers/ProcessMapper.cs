using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class ProcessMapper
{
    public static ProcessDto ToDto(this Process p) => new(
        Id:              p.Id,
        Name:            p.Name,
        StartDate:       p.StartDate,
        EndDate:         p.EndDate,
        PhaseId:         p.PhaseId,
        PhaseName:       p.Phase?.Name ?? string.Empty,
        ProcessStatusId: p.ProcessStatusId,
        ProcessStatus:   new ProcessStatusDto(
                             p.ProcessStatus?.Id         ?? 0,
                             p.ProcessStatus?.Name       ?? string.Empty,
                             p.ProcessStatus?.Percentage ?? 0),
        DependsOn:       p.Dependencies?.Select(d => new ProcessDependencySummaryDto(
                             d.DependsOnProcessId,
                             d.DependsOn?.Name ?? string.Empty))
                         ?? [],
        Issues:          p.Issues?.Select(i => new IssueDto(i.Id, i.Name)) ?? []
    );
}
