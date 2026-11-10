namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

/// <summary>
/// Replaceable rule: finds overlapping shift intervals per employee.
/// Interval: start = (Date + StartTime) interpreted in the shift's STORED TimeZoneId and converted to UTC,
/// end = start + DurationMinutes. End equal to the next start is NOT an overlap.
/// Every overlapping pair of the employee's rows is a conflict, whether the rows are new or existing.
/// </summary>
public interface IShiftOverlapChecker
{
    /// <summary>TimeZoneIds among the rows that cannot be resolved on this server. Callers must reject the request when any.</summary>
    List<string> FindUnresolvableTimeZones(IReadOnlyList<ScheduledShift> rows);

    /// <summary>
    /// rows: work rows only (day-off rows have no interval): the rows being written plus the existing rows that remain in the window.
    /// Stops after maxResults overlaps. Throws ArgumentException for an unresolvable TimeZoneId (check FindUnresolvableTimeZones first).
    /// </summary>
    List<RosterOverlapDto> FindOverlaps(IReadOnlyList<ScheduledShift> rows, int maxResults);
}

/// <summary>One work row for the overlap check. IsNew = written by this request (only decides which row of a pair is reported first).</summary>
public record ScheduledShift(
    int EmployeeId,
    DateOnly Date,
    int ShiftId,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId,
    bool IsNew
);
