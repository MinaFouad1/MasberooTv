using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public interface IVacationTypeService
{
    Task<ApiResponse<List<VacationTypeDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<VacationTypeDto>> CreateAsync(CreateVacationTypeDto dto, CancellationToken ct = default);
    Task<ApiResponse<VacationTypeDto>> UpdateAsync(int id, UpdateVacationTypeDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}