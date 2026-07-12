using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class IssueMapper
{
    public static IssueDetailDto ToDto(this Issue i) => new(
        i.Id, i.Name,
        i.Status,
        i.Date,
        i.DueDate,
        i.Priority,
        i.ProcessId, i.Process?.Name ?? string.Empty,
        i.CreatedByUserId,
        i.CreatedByUser?.UserName
    );

    public static RiskResponseDto ToRiskResponseDto(this Issue i) => new(
        i.Id,
        i.Name,
        i.Priority?.ToString(),
        i.Status.ToString(),
        i.Date,
        i.DueDate,
        i.CreatedByUser?.UserName
    );
}
