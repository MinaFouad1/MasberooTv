using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IVacationTypeRepository : IGenericRepository<VacationType>
{
    Task<List<VacationType>> GetAllOrderedAsync(CancellationToken ct = default);
    Task<VacationType?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
    Task<List<VacationType>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
}
