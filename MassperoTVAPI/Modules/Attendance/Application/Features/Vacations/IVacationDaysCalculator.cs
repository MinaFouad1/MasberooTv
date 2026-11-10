namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

/// <summary>
/// Calculates the vacation days charged for a period. Kept behind an interface so the rule
/// (holidays, days off, half days...) can be replaced without editing the services.
/// </summary>
public interface IVacationDaysCalculator
{
    Task<decimal> CalculateDaysAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
}