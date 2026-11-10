using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Holidays;

public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
{
    public HolidayRepository(ApplicationDbContext context) : base(context) { }
    public async Task<List<Holiday>> GetAllOrderedAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted)
                       .OrderBy(x => x.HolidayDate)
                       .ToListAsync(ct);

    public async Task<Holiday?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<bool> DateExistsAsync(DateOnly date, int? excludeId = null, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => !x.IsDeleted
                                   && x.HolidayDate == date
                                   && (excludeId == null || x.Id != excludeId.Value), ct);

    public async Task<List<DateOnly>> GetDatesBetweenAsync(DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted && x.HolidayDate >= fromDate && x.HolidayDate <= toDate)
                       .Select(x => x.HolidayDate)
                       .Distinct()
                       .ToListAsync(ct);
}
