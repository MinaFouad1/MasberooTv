using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;

public class RosterMonthRepository : GenericRepository<RosterMonth>, IRosterMonthRepository
{
    public RosterMonthRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RosterMonth?> GetForUpdateAsync(int? teamId, int year, int month, CancellationToken ct = default)
        => teamId is int id
            ? await _dbSet.FirstOrDefaultAsync(x => !x.IsDeleted && x.TeamId == id && x.Year == year && x.Month == month, ct)
            : await _dbSet.FirstOrDefaultAsync(x => !x.IsDeleted && x.TeamId == null && x.Year == year && x.Month == month, ct);

    public async Task<RosterMonth?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
}
