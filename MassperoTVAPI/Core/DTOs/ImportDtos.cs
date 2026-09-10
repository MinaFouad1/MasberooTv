using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.DTOs;

public record CandidateImportRowDto
{
    public int RowNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public int JobId { get; init; }
    public int StatusId { get; init; }
    public int SecurityClearanceId { get; init; }
    public string? CvFile { get; init; }
    public string? ReasonOfAccept { get; init; }
    public string? ReasonOfReject { get; init; }
    public bool Accepted { get; init; }
    public string? ApplicationUserId { get; init; }
    public Gender? Gender { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AlternatePhoneNumber { get; init; }
    public string? Nationality { get; init; }
    public string? NationalId { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }
    public DateTime? DateOfBeginning { get; init; }
    public DateTime? HiringDate { get; init; }
    public string? PreferredJobLocation { get; init; }
    public PreferredShift? PreferredShift { get; init; }
    public string? City { get; init; }
    public Country? Country { get; init; }
    public string? CurrentEmployer { get; init; }
    public string? CurrentPosition { get; init; }
    public int? YearsOfExperience { get; init; }
    public decimal? ExpectedSalary { get; init; }
    public decimal? CurrentSalary { get; init; }
    public string? NoticePeriod { get; init; }
    public string? Availability { get; init; }
}

public record ImportErrorDto
{
    public int RowNumber { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ImportResultDto
{
    public string FileName { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int Imported { get; init; }
    public int Failed { get; init; }
    public List<ImportErrorDto> Errors { get; init; } = [];
}
