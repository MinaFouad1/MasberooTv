using MassperoTVAPI.Modules.AttendanceModule.Domain.Common;

namespace MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

public class Shift : AuditableEntity
{
    public string   Name            { get; set; } = string.Empty;
    public TimeOnly StartTime       { get; set; }
    public int      DurationMinutes { get; set; }
    public int      BreakMinutes    { get; set; }
    public int      GraceMinutes    { get; set; }
    public string   TimeZoneId      { get; set; } = "Egypt Standard Time";
    public bool     IsActive        { get; set; }
}
