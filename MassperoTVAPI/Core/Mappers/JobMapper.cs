using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class JobMapper
{
    public static JobDto ToDto(this Job j) => new(
        j.Id,
        j.Name,
        j.Desc,
        j.CategoryId,
        j.Category?.Name   ?? string.Empty,
        j.OpenPositions,
        j.CreatedAt,
        j.EmploymentType,
        j.TargetHiringDate,
        j.LocationId,
        j.Location?.Name,
        Applicants:  j.Candidates.Count,
        Interviews:  j.Candidates.Count(c =>
            string.Equals(c.Status?.Name, "Under Vetting", StringComparison.OrdinalIgnoreCase))
    );
}
