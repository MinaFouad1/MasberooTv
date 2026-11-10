using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public interface IVacationBalanceService
{
    Task<ApiResponse<List<VacationBalanceDto>>> GetByEmployeeAsync(int employeeId, int? year, CancellationToken ct = default);
    Task<ApiResponse<List<VacationBalanceDto>>> CreateAsync(int employeeId, CreateVacationBalancesDto dto, CancellationToken ct = default);
}