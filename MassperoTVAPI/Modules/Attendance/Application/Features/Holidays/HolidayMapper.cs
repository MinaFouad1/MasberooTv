using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

public static class HolidayMapper
{
    public static HolidayDto ToDto(this Holiday h) => new(
        h.Id,
        h.Name,
        h.HolidayDate,
        h.IsPaid,
        h.HolidayType
    );
}