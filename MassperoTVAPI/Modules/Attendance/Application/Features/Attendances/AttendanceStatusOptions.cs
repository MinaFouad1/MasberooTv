namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

/// <summary>
/// Thresholds used by the attendance calculator (percent = WorkedMinutes / (DurationMinutes - BreakMinutes)).
/// Defaults live in code; registered through the options pattern.
/// </summary>
public class AttendanceStatusOptions
{
    /// <summary>percent at or above this value: Present.</summary>
    public double PresentThreshold { get; set; } = 0.90;

    /// <summary>percent at or above this value (and below PresentThreshold): HalfDay; below it: Absent.</summary>
    public double HalfDayThreshold { get; set; } = 0.50;
}
