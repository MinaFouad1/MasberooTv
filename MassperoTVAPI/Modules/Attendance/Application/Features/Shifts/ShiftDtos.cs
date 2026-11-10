namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

public record ShiftDto(
    int Id,
    string Name,
    TimeOnly StartTime,
    int DurationMinutes,
    int BreakMinutes,
    int GraceMinutes,
    string TimeZoneId,
    bool IsActive
);

public record CreateShiftDto
{
    public string? Name { get; init; }

    /// <summary>Start time as "HH:mm" or "HH:mm:ss".</summary>
    public string? StartTime { get; init; }

    public int DurationMinutes { get; init; }
    public int BreakMinutes { get; init; }
    public int GraceMinutes { get; init; }

    /// <summary>Windows/IANA time zone id. Defaults to "Egypt Standard Time" on create.</summary>
    public string? TimeZoneId { get; init; }

    /// <summary>Defaults to true on create.</summary>
    public bool? IsActive { get; init; }
}

public record UpdateShiftDto : CreateShiftDto;