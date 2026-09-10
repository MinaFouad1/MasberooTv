using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class InterviewMapper
{
    public static InterviewDto ToDto(this Interview i) => new(
        i.Id, i.Grade, i.Comments, i.CreatedAt, i.EvaluatorId, i.Evaluator?.UserName,
        i.CandidateId, i.Candidate?.Name ?? string.Empty,
        i.TypeId,      i.Type?.Name      ?? string.Empty,
        i.InterviewMode, i.InterviewDate, i.StartTime, i.EndTime
    );
}
