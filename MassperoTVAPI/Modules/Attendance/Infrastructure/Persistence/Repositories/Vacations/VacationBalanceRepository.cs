using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;

public class VacationBalanceRepository : GenericRepository<VacationBalance>, IVacationBalanceRepository
{
    public VacationBalanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<VacationBalance?> GetForUpdateAsync(int employeeId, int vacationTypeId, int year, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(x => !x.IsDeleted
                                              && x.EmployeeId == employeeId
                                              && x.VacationTypeId == vacationTypeId
                                              && x.Year == year, ct);

    public async Task<List<int>> GetExistingTypeIdsAsync(int employeeId, int year, IEnumerable<int> vacationTypeIds, CancellationToken ct = default)
    {
        var typeIds = vacationTypeIds.Distinct().ToList();
        return await _dbSet.AsNoTracking()
                           .Where(x => !x.IsDeleted
                                    && x.EmployeeId == employeeId
                                    && x.Year == year
                                    && typeIds.Contains(x.VacationTypeId))
                           .Select(x => x.VacationTypeId)
                           .ToListAsync(ct);
    }

    public async Task<List<VacationBalance>> GetByEmployeeAsync(int employeeId, int? year, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
                          .Include(x => x.VacationType)
                          .Where(x => !x.IsDeleted && x.EmployeeId == employeeId);

        if (year.HasValue)
            query = query.Where(x => x.Year == year.Value);

        return await query.OrderByDescending(x => x.Year)
                          .ThenBy(x => x.VacationType.Name)
                          .ToListAsync(ct);
    }

    public async Task AddTransactionAsync(VacationTransaction transaction, CancellationToken ct = default)
        => await _context.VacationTransactions.AddAsync(transaction, ct);
}
