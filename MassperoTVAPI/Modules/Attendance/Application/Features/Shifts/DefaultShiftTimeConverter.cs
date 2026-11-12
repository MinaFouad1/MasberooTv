using System.Collections.Concurrent;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

public class DefaultShiftTimeConverter : IShiftTimeConverter
{
    private readonly ConcurrentDictionary<string, TimeZoneInfo> _zones = new();

    public bool CanResolve(string timeZoneId)
        => TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _);

    /// <summary>
    /// Local wall time (Date + StartTime, DateTimeKind.Unspecified) in the shift's stored zone, converted to UTC.
    /// Daylight-saving edge cases never throw: TimeZoneInfo.GetUtcOffset returns the zone's standard offset for an
    /// ambiguous local time (repeated hour) and for an invalid local time (skipped hour). Those are the TimeZoneInfo defaults.
    /// An unresolvable zone id is NOT replaced by UTC: it throws, and callers reject such requests beforehand.
    /// (Moved unchanged from DefaultShiftOverlapChecker.)
    /// </summary>
    public DateTime ToUtc(DateOnly date, TimeOnly startTime, string timeZoneId)
    {
        if (!_zones.TryGetValue(timeZoneId, out var zone))
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var found))
                throw new ArgumentException($"Time zone '{timeZoneId}' cannot be resolved.", nameof(timeZoneId));
            zone = found;
            _zones[timeZoneId] = zone;
        }

        var local  = date.ToDateTime(startTime, DateTimeKind.Unspecified);
        var offset = zone.GetUtcOffset(local);
        return DateTime.SpecifyKind(local - offset, DateTimeKind.Utc);
    }
}
