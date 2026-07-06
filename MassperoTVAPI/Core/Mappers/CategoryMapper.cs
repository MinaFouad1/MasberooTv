using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name);

    //public static InterviewDto CV(this Interview x) => new(x.Id, x.Grade, x.Comments, x.CandidateId, null, x.TypeId, x.Type?.Name ?? string.Empty);


 
}

    //public int Id { get; set; }
    //public string? Grade { get; set; }
    //public string? Comments { get; set; }

    //// FK → Candidate
    //public int CandidateId { get; set; }
    //public Candidate Candidate { get; set; } = null!;

    //// FK → InterviewType
    //public int TypeId { get; set; }
    //public InterviewType Type { get; set; } = null!;
