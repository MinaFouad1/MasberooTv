using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

public interface IShiftService
{
    Task<ApiResponse<List<ShiftDto>>> GetAllAsync(string? name, bool? isActive, CancellationToken ct = default);
    Task<ApiResponse<ShiftDto>> CreateAsync(CreateShiftDto dto, CancellationToken ct = default);
    Task<ApiResponse<ShiftDto>> UpdateAsync(int id, UpdateShiftDto dto, CancellationToken ct = default);
}