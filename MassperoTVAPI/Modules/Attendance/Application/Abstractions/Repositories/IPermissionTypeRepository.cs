using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IPermissionTypeRepository : IGenericRepository<PermissionType>
{
    Task<List<PermissionTypeDto>> SearchAsync(bool? isActive, CancellationToken ct = default);
    Task<PermissionType?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
    Task<List<PermissionType>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
}
