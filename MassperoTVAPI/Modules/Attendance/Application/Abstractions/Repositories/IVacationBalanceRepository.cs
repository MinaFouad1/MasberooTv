using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IVacationBalanceRepository : IGenericRepository<VacationBalance>
{
    Task<VacationBalance?> GetForUpdateAsync(int employeeId, int vacationTypeId, int year, CancellationToken ct = default);
    Task<List<int>> GetExistingTypeIdsAsync(int employeeId, int year, IEnumerable<int> vacationTypeIds, CancellationToken ct = default);
    Task<List<VacationBalance>> GetByEmployeeAsync(int employeeId, int? year, CancellationToken ct = default);
    Task AddTransactionAsync(VacationTransaction transaction, CancellationToken ct = default);
}
