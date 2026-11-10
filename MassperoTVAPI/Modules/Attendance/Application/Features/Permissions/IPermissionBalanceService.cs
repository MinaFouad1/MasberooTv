using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public interface IPermissionBalanceService
{
    Task<ApiResponse<List<PermissionBalanceDto>>> GetByEmployeeAsync(int employeeId, int? year, int? month, CancellationToken ct = default);
    Task<ApiResponse<List<PermissionBalanceDto>>> CreateAsync(int employeeId, CreatePermissionBalancesDto dto, CancellationToken ct = default);
    Task<ApiResponse<PermissionBalanceDto>> UpdateAsync(int employeeId, UpdatePermissionBalanceDto dto, CancellationToken ct = default);
}
