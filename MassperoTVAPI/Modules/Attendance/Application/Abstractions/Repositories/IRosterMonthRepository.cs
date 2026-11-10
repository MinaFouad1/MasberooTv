using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IRosterMonthRepository : IGenericRepository<RosterMonth>
{
    /// <summary>Non-deleted header for (teamId or company-wide when null, year, month), TRACKED.</summary>
    Task<RosterMonth?> GetForUpdateAsync(int? teamId, int year, int month, CancellationToken ct = default);
    Task<RosterMonth?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
}
