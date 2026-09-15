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
        j.DeadLineDate,
        j.IsOpend,
        j.IsOpend ? "Opened" : "Closed",
        j.LocationId,
        j.Location?.Name,
        j.HiringManagerId,
        j.HiringManager?.UserName,
        Applicants:       j.Candidates.Count(c => !string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase)),
        Interviews:       j.Candidates.Count(c => (c.Interviews != null && c.Interviews.Any()) || string.Equals(c.Status?.Name, "Under Vetting", StringComparison.OrdinalIgnoreCase)),
        Offers:           j.Candidates.Count(c => string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase)),
        Responsibilities: j.Responsibilities,
        RequiredSkills:   j.RequiredSkills,
        ExperienceLevel:  j.ExperienceLevel,
        Education:        j.Education,
        Languages:        j.Languages
    );
}
