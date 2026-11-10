using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionBalanceRepository : GenericRepository<PermissionBalance>, IPermissionBalanceRepository
{
    public PermissionBalanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<PermissionBalanceDto>> GetByEmployeeAsync(int employeeId, int? year, int? month, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted
                                && x.EmployeeId == employeeId
                                && (year == null || x.Year == year.Value)
                                && (month == null || x.Month == month.Value))
                       .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.PermissionType.Name)
                       .Select(x => new PermissionBalanceDto(
                           x.PermissionTypeId,
                           x.PermissionType.Name,
                           x.Year,
                           x.Month,
                           x.AllowedMinutes,
                           x.UsedMinutes,
                           x.AllowedMinutes > x.UsedMinutes ? x.AllowedMinutes - x.UsedMinutes : 0,
                           x.UsedMinutes > x.AllowedMinutes ? x.UsedMinutes - x.AllowedMinutes : 0))
                       .ToListAsync(ct);

    public async Task<List<PermissionBalanceConflictDto>> GetExistingAsync(int employeeId, int year, IEnumerable<int> permissionTypeIds,
        IEnumerable<int> months, CancellationToken ct = default)
    {
        var typeIdList = permissionTypeIds.Distinct().ToList();
        var monthList  = months.Distinct().ToList();
        return await _dbSet.AsNoTracking()
                           .Where(x => !x.IsDeleted
                                    && x.EmployeeId == employeeId
                                    && x.Year == year
                                    && typeIdList.Contains(x.PermissionTypeId)
                                    && monthList.Contains(x.Month))
                           .OrderBy(x => x.PermissionTypeId).ThenBy(x => x.Month)
                           .Select(x => new PermissionBalanceConflictDto(x.PermissionTypeId, x.Year, x.Month))
                           .ToListAsync(ct);
    }

    public async Task<PermissionBalance?> GetForUpdateAsync(int employeeId, int permissionTypeId, int year, int month, CancellationToken ct = default)
        => await _dbSet.Include(x => x.PermissionType)
                       .FirstOrDefaultAsync(x => !x.IsDeleted
                                              && x.EmployeeId == employeeId
                                              && x.PermissionTypeId == permissionTypeId
                                              && x.Year == year
                                              && x.Month == month, ct);

    public async Task AddRangeAsync(IEnumerable<PermissionBalance> balances, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(balances, ct);
}
