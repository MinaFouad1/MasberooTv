using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Mappers;

public static class CandidateMapper
{
    public static CandidateDto ToDto(this Candidate c, string? cvBaseUrl = null) => new(
        c.Id, c.Name, BuildCvUrl(c.CvFile, cvBaseUrl), c.ReasonOfAccept, c.ReasonOfReject, c.Accepted,
        c.JobId,               c.Job?.Name               ?? string.Empty,
        c.StatusId,            c.Status?.Name            ?? string.Empty,
        c.SecurityClearanceId, c.SecurityClearance?.Name ?? string.Empty,
        c.ApplicationUserId,   c.User?.UserName
    );

    private static string? BuildCvUrl(string? fileName, string? cvBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (Uri.TryCreate(fileName, UriKind.Absolute, out _))
            return fileName;

        return string.IsNullOrWhiteSpace(cvBaseUrl)
            ? fileName
            : $"{cvBaseUrl.TrimEnd('/')}/{fileName}";
    }
}
