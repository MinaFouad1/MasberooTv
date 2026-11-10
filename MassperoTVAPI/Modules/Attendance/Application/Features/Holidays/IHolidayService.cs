using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

public interface IHolidayService
{
    Task<ApiResponse<List<HolidayDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<HolidayDto>> CreateAsync(CreateHolidayDto dto, CancellationToken ct = default);
    Task<ApiResponse<HolidayDto>> UpdateAsync(int id, UpdateHolidayDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}