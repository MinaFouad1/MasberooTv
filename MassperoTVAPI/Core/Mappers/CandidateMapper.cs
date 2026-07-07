using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class CandidateMapper
{
    private const string HrTypeName        = "HR";
    private const string TechnicalTypeName = "Technical";

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
    /// Builds a full URL for a stored file name using a base URL from configuration.
    /// Returns null if fileName is empty. Returns fileName as-is if it is already an absolute URI.
    /// </summary>
    private static string? BuildFileUrl(string? fileName, string? baseUrl)
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

