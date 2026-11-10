using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IPermissionBalanceRepository : IGenericRepository<PermissionBalance>
{
    Task<List<PermissionBalanceDto>> GetByEmployeeAsync(int employeeId, int? year, int? month, CancellationToken ct = default);
    Task<List<PermissionBalanceConflictDto>> GetExistingAsync(int employeeId, int year, IEnumerable<int> permissionTypeIds, IEnumerable<int> months, CancellationToken ct = default);
    Task<PermissionBalance?> GetForUpdateAsync(int employeeId, int permissionTypeId, int year, int month, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PermissionBalance> balances, CancellationToken ct = default);
}
