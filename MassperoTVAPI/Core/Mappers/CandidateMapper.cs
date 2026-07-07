using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class CandidateMapper
{
    private const string HrTypeName        = "HR";
    private const string TechnicalTypeName = "Technical";

    // ── List DTO (used by paged GET and status/security patch endpoints) ──────
    public static CandidateDto ToDto(
        this Candidate c,
        string? cvBaseUrl      = null,
        string? profileBaseUrl = null) => new(
        c.Id,
        c.Name,
        BuildFileUrl(c.CvFile, cvBaseUrl),
        BuildFileUrl(c.Profile, profileBaseUrl),
        c.ReasonOfAccept,
        c.ReasonOfReject,
        c.Accepted,
        c.JobId,
        c.Job?.Name           ?? string.Empty,
        c.Job?.CategoryId     ?? 0,
        c.Job?.Category?.Name ?? string.Empty,
        c.StatusId,
        c.Status?.Name        ?? string.Empty,
        c.SecurityClearanceId,
        c.SecurityClearance?.Name ?? string.Empty,
        c.ApplicationUserId,
        c.User?.UserName,
        c.HiringDate,
        GetScoreByType(c.Interviews, HrTypeName),
        GetScoreByType(c.Interviews, TechnicalTypeName)
    );

    // ── Detail DTO (used by GET /api/candidates/{id}) ─────────────────────────
    public static CandidateDetailDto ToDetailDto(
        this Candidate c,
        int     rankingPosition,
        string? cvBaseUrl      = null,
        string? profileBaseUrl = null)
    {
        var overallScore       = ComputeOverallScore(c.Interviews);
        var hiringProbability  = ComputeHiringProbability(overallScore);

        return new CandidateDetailDto(
            // Identity
            Id:                     c.Id,
            Name:                   c.Name,
            CandidateCode:          BuildCandidateCode(c.Id),

            // Files
            CvFile:                 BuildFileUrl(c.CvFile, cvBaseUrl),
            ProfileImage:           BuildFileUrl(c.Profile, profileBaseUrl),

            // Job
            JobId:                  c.JobId,
            JobName:                c.Job?.Name           ?? string.Empty,
            JobDescription:         c.Job?.Desc,
            EmploymentType:         c.Job?.EmploymentType,
            CategoryId:             c.Job?.CategoryId     ?? 0,
            CategoryName:           c.Job?.Category?.Name ?? string.Empty,
            WorkLocation:           c.Job?.Location?.Name,
            JobCreatedAt:           c.Job?.CreatedAt      ?? DateTime.MinValue,
            TargetHiringDate:       c.Job?.TargetHiringDate,

            // Status & dates
            StatusId:               c.StatusId,
            StatusName:             c.Status?.Name            ?? string.Empty,
            SecurityClearanceId:    c.SecurityClearanceId,
            SecurityClearanceName:  c.SecurityClearance?.Name ?? string.Empty,
            HiringDate:             c.HiringDate,

            // HR user
            ApplicationUserId:      c.ApplicationUserId,
            ApplicationUserName:    c.User?.UserName,

            // Notes
            ReasonOfAccept:         c.ReasonOfAccept,
            ReasonOfReject:         c.ReasonOfReject,
            Accepted:               c.Accepted,

            // Scores
            HrScore:                GetScoreByType(c.Interviews, HrTypeName),
            TechnicalScore:         GetScoreByType(c.Interviews, TechnicalTypeName),
            OverallScore:           overallScore.HasValue ? Math.Round(overallScore.Value, 2) : null,
            HiringProbability:      hiringProbability,

            // Ranking
            RankingPosition:        rankingPosition,

            // All interviews
            Interviews:             c.Interviews
                                     .OrderBy(i => i.CreatedAt)
                                     .Select(i => new InterviewSummaryDto(
                                         i.Id,
                                         i.Type?.Name  ?? string.Empty,
                                         i.Grade,
                                         i.Comments,
                                         i.CreatedAt))
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the grade of the first interview whose type name contains the given keyword
    /// (case-insensitive). Returns null if no matching interview exists.
    /// </summary>
    private static string? GetScoreByType(ICollection<Interview> interviews, string typeKeyword)
        => interviews
            .FirstOrDefault(i => i.Type?.Name != null &&
                                 i.Type.Name.Contains(typeKeyword, StringComparison.OrdinalIgnoreCase))
            ?.Grade;

    /// <summary>
    /// Averages the numeric grades of all interviews whose Grade parses as a decimal.
    /// Returns null if no parseable grades exist.
    /// </summary>
    public static decimal? ComputeOverallScore(ICollection<Interview> interviews)
    {
        var scores = interviews
            .Select(i => decimal.TryParse(i.Grade, out var g) ? (decimal?)g : null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        return scores.Count == 0 ? null : scores.Average();
    }

    /// <summary>
    /// Maps overall score to a human-readable hiring probability label.
    /// &lt;50 → Low | 50–74 → Medium | 75–89 → High | ≥90 → Very High | null → N/A
    /// </summary>
    private static string ComputeHiringProbability(decimal? overallScore) => overallScore switch
    {
        null         => "N/A",
        < 50m        => "Low",
        < 75m        => "Medium",
        < 90m        => "High",
        _            => "Very High"
    };

    /// <summary>Formats the candidate code displayed in the UI: CAN-YYYY-NNNNN.</summary>
    public static string BuildCandidateCode(int candidateId)
        => $"CAN-{DateTime.UtcNow.Year}-{candidateId:D4}";

    /// <summary>
    /// Builds a full URL for a stored file name using a base URL from configuration.
    /// Returns null if fileName is empty. Returns fileName as-is if it is already an absolute URI.
    /// </summary>
    public static string? BuildFileUrl(string? fileName, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (Uri.TryCreate(fileName, UriKind.Absolute, out _))
            return fileName;

        return string.IsNullOrWhiteSpace(baseUrl)
            ? fileName
            : $"{baseUrl.TrimEnd('/')}/{fileName}";
    }
}

