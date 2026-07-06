using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class InterviewMapper
{
    public static InterviewDto ToDto(this Interview i) => new(
        i.Id, i.Grade, i.Comments,
        i.CandidateId, i.Candidate?.Name ?? string.Empty,
        i.TypeId,      i.Type?.Name      ?? string.Empty
    );
}
