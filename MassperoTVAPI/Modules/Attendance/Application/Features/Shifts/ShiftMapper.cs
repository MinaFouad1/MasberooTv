using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

public static class ShiftMapper
{
    public static ShiftDto ToDto(this Shift s) => new(
        s.Id,
        s.Name,
        s.StartTime,
        s.DurationMinutes,
        s.BreakMinutes,
        s.GraceMinutes,
        s.TimeZoneId,
        s.IsActive
    );
}