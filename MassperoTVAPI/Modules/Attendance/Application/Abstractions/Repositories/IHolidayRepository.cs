using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IHolidayRepository : IGenericRepository<Holiday>
{
    Task<List<Holiday>> GetAllOrderedAsync(CancellationToken ct = default);
    Task<Holiday?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
    Task<bool> DateExistsAsync(DateOnly date, int? excludeId = null, CancellationToken ct = default);
    Task<List<DateOnly>> GetDatesBetweenAsync(DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
}
