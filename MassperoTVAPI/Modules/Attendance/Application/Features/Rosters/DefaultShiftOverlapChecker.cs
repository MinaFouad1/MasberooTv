namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

public class DefaultShiftOverlapChecker : IShiftOverlapChecker
{
    public List<string> FindUnresolvableTimeZones(IReadOnlyList<ScheduledShift> rows)
        => rows.Select(r => r.TimeZoneId)
               .Distinct()
               .Where(id => !TimeZoneInfo.TryFindSystemTimeZoneById(id, out _))
               .OrderBy(id => id)
               .ToList();

    public List<RosterOverlapDto> FindOverlaps(IReadOnlyList<ScheduledShift> rows, int maxResults)
    {
        var overlaps = new List<RosterOverlapDto>();
        var zones    = new Dictionary<string, TimeZoneInfo>();

        foreach (var employeeRows in rows.GroupBy(r => r.EmployeeId).OrderBy(g => g.Key))
        {
            var intervals = employeeRows
                .Select(r =>
                {
                    var start = ToUtc(r.Date, r.StartTime, r.TimeZoneId, zones);
                    return (Row: r, Start: start, End: start.AddMinutes(r.DurationMinutes));
                })
                .OrderBy(x => x.Start).ThenBy(x => x.End).ThenBy(x => x.Row.ShiftId)
                .ToList();

            // Sorted by start; compare each interval with the earlier interval that ends last.
            // If any earlier interval overlaps the current one, that one does too, so no overlap is missed.
            (ScheduledShift Row, DateTime Start, DateTime End)? latest = null;
            foreach (var current in intervals)
            {
                if (latest is { } previous && current.Start < previous.End)
                {
                    var (row, other) = previous.Row.IsNew && !current.Row.IsNew ? (previous.Row, current.Row) : (current.Row, previous.Row);
                    overlaps.Add(new RosterOverlapDto(row.EmployeeId, row.Date, row.ShiftId, other.ShiftId, other.Date));
                    if (overlaps.Count >= maxResults)
                        return overlaps;
                }

                if (latest is null || current.End > latest.Value.End)
                    latest = current;
            }
        }

        return overlaps;
    }

    /// <summary>
    /// Local wall time (Date + StartTime, DateTimeKind.Unspecified) in the shift's stored zone, converted to UTC.
    /// Daylight-saving edge cases never throw: TimeZoneInfo.GetUtcOffset returns the zone's standard offset for an
    /// ambiguous local time (repeated hour) and for an invalid local time (skipped hour). Those are the TimeZoneInfo defaults.
    /// An unresolvable zone id is NOT replaced by UTC: it throws, and callers reject such requests beforehand.
    /// </summary>
    private static DateTime ToUtc(DateOnly date, TimeOnly startTime, string timeZoneId, Dictionary<string, TimeZoneInfo> zones)
    {
        if (!zones.TryGetValue(timeZoneId, out var zone))
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var found))
                throw new ArgumentException($"Time zone '{timeZoneId}' cannot be resolved.", nameof(timeZoneId));
            zone = found;
            zones[timeZoneId] = zone;
        }

        var local  = date.ToDateTime(startTime, DateTimeKind.Unspecified);
        var offset = zone.GetUtcOffset(local);
        return DateTime.SpecifyKind(local - offset, DateTimeKind.Utc);
    }
}
