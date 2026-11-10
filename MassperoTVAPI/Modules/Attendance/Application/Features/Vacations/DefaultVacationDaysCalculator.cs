using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

/// <summary>
/// Calendar days from FromDate to ToDate inclusive, minus non-deleted Holiday dates,
/// minus the employee's non-deleted ShiftAssignment dates with DayType = DayOff.
/// Whole days only (half days are not supported yet).
/// </summary>
public class DefaultVacationDaysCalculator : IVacationDaysCalculator
{
    private readonly IUnitOfWork _uow;

    public DefaultVacationDaysCalculator(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<decimal> CalculateDaysAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        if (toDate < fromDate)
            return 0;

        var holidays = (await _uow.Holidays.GetDatesBetweenAsync(fromDate, toDate, ct)).ToHashSet();
        var daysOff  = (await _uow.ShiftAssignments.GetDayOffDatesAsync(employeeId, fromDate, toDate, ct)).ToHashSet();

        var days = 0;
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            if (!holidays.Contains(date) && !daysOff.Contains(date))
                days++;
        }

        return days;
    }
}