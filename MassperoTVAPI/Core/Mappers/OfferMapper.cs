using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.Mappers;

public static class OfferMapper
{
    private const string HrTypeName        = "HR";
    private const string TechnicalTypeName = "Technical";

    // ── List / table row ──────────────────────────────────────────────────────
    public static OfferDto ToDto(this Offer o, string? profileBaseUrl = null) => new(
        Id:             o.Id,
        CandidateId:    o.CandidateId,
        CandidateName:  o.Candidate?.Name             ?? string.Empty,
        CandidateCode:  BuildCandidateCode(o.CandidateId),
        JobId:          o.Candidate?.JobId            ?? 0,
        JobName:        o.Candidate?.Job?.Name        ?? string.Empty,
        CategoryId:     o.Candidate?.Job?.CategoryId  ?? 0,
        DepartmentName: o.Candidate?.Job?.Category?.Name ?? string.Empty,
        EmploymentType: o.Candidate?.Job?.EmploymentType,
        WorkLocation:   o.Candidate?.Job?.Location?.Name,
        OfferStatus:    o.OfferStatusId.ToString(),
        OfferStatusId:  (int)o.OfferStatusId,
        ProposedSalary: o.ProposedSalary,
        StartDate:      o.StartDate,
        OfferDate:      o.OfferDate,
        ExpiryDate:     o.ExpiryDate,
        Benefits:       o.Benefits,
        ProfileImage:   CandidateMapper.BuildFileUrl(o.Candidate?.Profile, profileBaseUrl),
        ReasonOfRejected: o.ReasonOfRejected,
        CreatedAt:      o.CreatedAt,
        CreatedBy:      o.CreatedBy
    );

    // ── Preview panel ─────────────────────────────────────────────────────────
    public static OfferPreviewDto ToPreviewDto(this Offer o, int rank) => new(
        OfferId:        o.Id,
        OfferStatus:    o.OfferStatusId.ToString(),
        OfferStatusId:  (int)o.OfferStatusId,
        ProposedSalary: o.ProposedSalary,
        StartDate:      o.StartDate,
        OfferDate:      o.OfferDate,
        ExpiryDate:     o.ExpiryDate,
        Benefits:       o.Benefits,
        CandidateId:    o.CandidateId,
        CandidateName:  o.Candidate?.Name             ?? string.Empty,
        CandidateCode:  BuildCandidateCode(o.CandidateId),
        JobId:          o.Candidate?.JobId            ?? 0,
        JobName:        o.Candidate?.Job?.Name        ?? string.Empty,
        DepartmentName: o.Candidate?.Job?.Category?.Name ?? string.Empty,
        EmploymentType: o.Candidate?.Job?.EmploymentType,
        WorkLocation:   o.Candidate?.Job?.Location?.Name,
        HrScore:        GetScoreByType(o.Candidate?.Interviews, HrTypeName),
        TechnicalScore: GetScoreByType(o.Candidate?.Interviews, TechnicalTypeName),
        CandidateRank:  rank
    );

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Formats candidate code the same way the UI shows it: CAN-YYYY-NNNNN.</summary>
    private static string BuildCandidateCode(int candidateId)
        => $"CAN-{DateTime.UtcNow.Year}-{candidateId:D4}";

    private static string? GetScoreByType(ICollection<Interview>? interviews, string typeKeyword)
        => interviews?
            .FirstOrDefault(i => i.Type?.Name != null &&
                                 i.Type.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase))
            ?.Grade;
}
