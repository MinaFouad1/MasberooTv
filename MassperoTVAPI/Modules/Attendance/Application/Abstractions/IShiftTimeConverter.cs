namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>
/// Converts a shift's local start (date + StartTime in the shift's STORED TimeZoneId) to a UTC instant.
/// Shared by the roster overlap check and the attendance calculation so both use exactly the same rule.
/// </summary>
public interface IShiftTimeConverter
{
    /// <summary>True when the time zone id can be resolved on this server.</summary>
    bool CanResolve(string timeZoneId);

    /// <summary>
    /// Local wall time (date + startTime, DateTimeKind.Unspecified) in the zone, converted to UTC (DateTimeKind.Utc).
    /// Throws ArgumentException for an unresolvable zone id (check <see cref="CanResolve"/> first).
    /// </summary>
    DateTime ToUtc(DateOnly date, TimeOnly startTime, string timeZoneId);
}
