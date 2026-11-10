using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionTypeRepository : GenericRepository<PermissionType>, IPermissionTypeRepository
{
    public PermissionTypeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<PermissionTypeDto>> SearchAsync(bool? isActive, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted && (isActive == null || x.IsActive == isActive.Value))
                       .OrderBy(x => x.Name)
                       .Select(x => new PermissionTypeDto(x.Id, x.Name, x.DefaultMonthlyMinutes, x.IsActive))
                       .ToListAsync(ct);

    public async Task<PermissionType?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<List<PermissionType>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        return await _dbSet.AsNoTracking().Where(x => !x.IsDeleted && idList.Contains(x.Id)).ToListAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var lowered = name.ToLower();
        return await _dbSet.AnyAsync(x => !x.IsDeleted
                                       && x.Name.ToLower() == lowered
                                       && (excludeId == null || x.Id != excludeId.Value), ct);
    }

    public async Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        => await _context.PermissionRequests.AnyAsync(r => r.PermissionTypeId == id && !r.IsDeleted, ct)
        || await _context.PermissionBalances.AnyAsync(b => b.PermissionTypeId == id && !b.IsDeleted, ct);
}
