using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public interface IPermissionRequestService
{
    Task<ApiResponse<List<PermissionRequestDto>>> GetAllAsync(int? employeeId, int? permissionTypeId, RequestStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<ApiResponse<PermissionRequestDto>> CreateAsync(CreatePermissionRequestDto dto, CancellationToken ct = default);
    Task<ApiResponse<PermissionRequestDto>> WithdrawAsync(int id, CancellationToken ct = default);
}
