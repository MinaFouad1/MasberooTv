using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public static class VacationMapper
{
    public static VacationTypeDto ToDto(this VacationType t) => new(
        t.Id,
        t.Name,
        t.IsPaid,
        t.DefaultDays,
        t.CanCarryOver
    );

    public static VacationBalanceDto ToDto(this VacationBalance b, string vacationTypeName) => new(
        b.VacationTypeId,
        vacationTypeName,
        b.Year,
        b.Entitled,
        b.CarriedOver,
        b.Used,
        b.Entitled + b.CarriedOver - b.Used
    );

    public static VacationRequestDto ToDto(this VacationRequest r, string vacationTypeName) => new(
        r.Id,
        r.EmployeeId,
        r.VacationTypeId,
        vacationTypeName,
        r.FromDate,
        r.ToDate,
        r.Days,
        r.Status,
        r.Reason,
        r.CreatedAt
    );
}