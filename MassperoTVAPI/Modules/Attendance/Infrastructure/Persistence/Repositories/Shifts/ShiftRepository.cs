using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Shifts;

public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext context) : base(context) { }
    public async Task<List<Shift>> SearchAsync(string? name, bool? isActive, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public async Task<Shift?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var lowered = name.ToLower();
        return await _dbSet.AnyAsync(x => !x.IsDeleted
                                       && x.Name.ToLower() == lowered
                                       && (excludeId == null || x.Id != excludeId.Value), ct);
    }

    public async Task<List<Shift>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new List<Shift>();

        return await _dbSet.AsNoTracking().Where(x => !x.IsDeleted && idList.Contains(x.Id)).ToListAsync(ct);
    }
}
