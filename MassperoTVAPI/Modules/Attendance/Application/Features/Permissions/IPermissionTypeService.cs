using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public interface IPermissionTypeService
{
    Task<ApiResponse<List<PermissionTypeDto>>> GetAllAsync(bool? isActive, CancellationToken ct = default);
    Task<ApiResponse<PermissionTypeDto>> CreateAsync(CreatePermissionTypeDto dto, CancellationToken ct = default);
    Task<ApiResponse<PermissionTypeDto>> UpdateAsync(int id, UpdatePermissionTypeDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
