using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

public interface IAttendanceService
{
    Task<ApiResponse<AttendanceDto>> CreateManualAsync(CreateManualAttendanceDto dto, CancellationToken ct = default);

    Task<ApiResponse<AttendancePagedResultDto>> SearchAsync(AttendanceSearchQueryDto query, CancellationToken ct = default);
}
