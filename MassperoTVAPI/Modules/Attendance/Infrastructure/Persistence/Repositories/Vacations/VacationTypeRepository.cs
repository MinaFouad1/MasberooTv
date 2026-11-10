using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;

public class VacationTypeRepository : GenericRepository<VacationType>, IVacationTypeRepository
{
    public VacationTypeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<VacationType>> GetAllOrderedAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted)
                       .OrderBy(x => x.Name)
                       .ToListAsync(ct);

    public async Task<VacationType?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<List<VacationType>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        return await _dbSet.Where(x => !x.IsDeleted && idList.Contains(x.Id)).ToListAsync(ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var lowered = name.ToLower();
        return await _dbSet.AnyAsync(x => !x.IsDeleted
                                       && x.Name.ToLower() == lowered
                                       && (excludeId == null || x.Id != excludeId.Value), ct);
    }

    public async Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        => await _context.VacationRequests.AnyAsync(r => r.VacationTypeId == id && !r.IsDeleted, ct)
        || await _context.VacationBalances.AnyAsync(b => b.VacationTypeId == id && !b.IsDeleted, ct);
}
