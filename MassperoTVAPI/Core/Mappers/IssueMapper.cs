using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class IssueMapper
{
    public static IssueDetailDto ToDto(this Issue i) => new(
        i.Id, i.Name,
        i.Resolved,
        i.Date,
        i.Priority,
        i.ProcessId, i.Process?.Name ?? string.Empty
    );

    public static RiskResponseDto ToRiskResponseDto(this Issue i) => new(
        i.Id,
        i.Name,
        i.Priority?.ToString(),
        i.Resolved == true ? "Accepted" : "Not Accepted",
        i.Date
    );
}
