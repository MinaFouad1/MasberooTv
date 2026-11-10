using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

public interface IRosterService
{
    Task<ApiResponse<RosterAssignmentResultDto>> AssignAsync(BulkRosterAssignmentDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<RosterRowDto>>> EditDayAsync(int employeeId, DateOnly date, EditRosterDayDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<RosterEmployeeScheduleDto>>> GetScheduleAsync(int year, int month, int? teamId, int? employeeId, CancellationToken ct = default);
}
