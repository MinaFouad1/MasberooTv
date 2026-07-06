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
