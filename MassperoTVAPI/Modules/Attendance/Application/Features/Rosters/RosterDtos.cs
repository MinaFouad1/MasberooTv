using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

// -- Bulk assign (input) ----------------------------------------------------

public record BulkRosterAssignmentDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int? TeamId { get; init; }
    public List<int>? EmployeeIds { get; init; }

    /// <summary>"Skip" (default) or "Replace".</summary>
    public string? ConflictMode { get; init; }

    /// <summary>Give EXACTLY ONE of Days or WeeklyPattern.</summary>
    public List<RosterDayInputDto>? Days { get; init; }
    public RosterWeeklyPatternDto? WeeklyPattern { get; init; }
}

public record RosterDayInputDto
{
    /// <summary>Date as "yyyy-MM-dd".</summary>
    public DateOnly? Date { get; init; }
    public DayType? DayType { get; init; }
    public List<int>? ShiftIds { get; init; }
    public string? Note { get; init; }
}

public record RosterWeeklyPatternDto
{
    /// <summary>Optional, default: first day of the month.</summary>
    public DateOnly? DateFrom { get; init; }

    /// <summary>Optional, default: last day of the month.</summary>
    public DateOnly? DateTo { get; init; }

    public List<RosterWeeklyItemDto>? Items { get; init; }
}

public record RosterWeeklyItemDto
{
    public DayOfWeek? DayOfWeek { get; init; }
    public DayType? DayType { get; init; }
    public List<int>? ShiftIds { get; init; }
    public string? Note { get; init; }
}

// -- Edit one employee day (input) ------------------------------------------

public record EditRosterDayDto
{
    public DayType? DayType { get; init; }
    public List<int>? ShiftIds { get; init; }
    public string? Note { get; init; }
}

// -- Results ------------------------------------------------------------------

public record RosterAssignmentResultDto(
    int RosterMonthId,
    int EmployeesCount,
    int CreatedCount,
    int SkippedCount,
    int ReplacedCount
);

public record RosterOverlapDto(
    int EmployeeId,
    DateOnly Date,
    int ShiftId,
    int ConflictsWithShiftId,
    DateOnly ConflictsWithDate
);

public record RosterRowDto(
    int Id,
    DateOnly Date,
    DayType DayType,
    int? ShiftId,
    string? ShiftName,
    TimeOnly? StartTime,
    int? DurationMinutes,
    string? Note
);

public record RosterDayScheduleDto(
    DateOnly Date,
    List<RosterRowDto> Rows
);

public record RosterEmployeeScheduleDto(
    int EmployeeId,
    string EmployeeName,
    List<RosterDayScheduleDto> Days
);

/// <summary>Flat read model projected by the repository (one row per assignment).</summary>
public record RosterScheduleRowDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    DateOnly Date,
    DayType DayType,
    int? ShiftId,
    string? ShiftName,
    TimeOnly? StartTime,
    int? DurationMinutes,
    string? Note
);
