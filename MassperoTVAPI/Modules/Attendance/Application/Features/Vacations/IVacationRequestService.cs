using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public interface IVacationRequestService
{
    Task<ApiResponse<List<VacationRequestDto>>> GetAllAsync(int? employeeId, string? status, CancellationToken ct = default);
    Task<ApiResponse<VacationRequestDto>> CreateAsync(CreateVacationRequestDto dto, CancellationToken ct = default);
    Task<ApiResponse<VacationRequestDto>> WithdrawAsync(int id, CancellationToken ct = default);
}