using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class IssueMapper
{
    public static IssueDetailDto ToDto(this Issue i) => new(
        i.Id, i.Name,
        i.Resolved,
        i.Date,
        i.ProcessId, i.Process?.Name ?? string.Empty
    );
}
