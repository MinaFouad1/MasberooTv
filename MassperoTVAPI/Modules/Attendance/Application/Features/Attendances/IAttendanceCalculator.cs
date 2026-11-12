using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

/// <summary>
/// Replaceable rule, pure (no database access): expected times, late / early-leave / worked minutes and status of one attendance day.
/// Approved permissions are NOT applied here.
/// </summary>
public interface IAttendanceCalculator
{
    AttendanceCalculationResult Calculate(AttendanceCalculationInput input);
}

/// <summary>The shift values the calculator needs.</summary>
public record AttendanceShiftInfo(
    TimeOnly StartTime,
    int DurationMinutes,
    int BreakMinutes,
    int GraceMinutes,
    string TimeZoneId
);

/// <summary>Shift is null for an unscheduled day. WorkDate is the date the shift STARTS.</summary>
public record AttendanceCalculationInput(
    AttendanceShiftInfo? Shift,
    DateOnly WorkDate,
    DateTimeOffset? CheckIn,
    DateTimeOffset? CheckOut,
    bool IsHoliday,
    bool HasApprovedVacation,
    DateTimeOffset Now
);

/// <summary>ExpectedStart / ExpectedEnd are UTC and null when there is no shift.</summary>
public record AttendanceCalculationResult(
    DateTimeOffset? ExpectedStart,
    DateTimeOffset? ExpectedEnd,
    int LateMinutes,
    int EarlyLeaveMinutes,
    int WorkedMinutes,
    AttendanceStatus Status
);
