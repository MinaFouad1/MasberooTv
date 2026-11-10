using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IShiftRepository : IGenericRepository<Shift>
{
    Task<List<Shift>> SearchAsync(string? name, bool? isActive, CancellationToken ct = default);
    Task<Shift?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<List<Shift>> GetByIdsNotDeletedAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
