using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;

public class VacationRequestRepository : GenericRepository<VacationRequest>, IVacationRequestRepository
{
    public VacationRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> HasApprovedOverlapAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(x => !x.IsDeleted
                                   && x.EmployeeId == employeeId
                                   && x.Status == RequestStatus.Approved
                                   && x.FromDate <= toDate
                                   && x.ToDate >= fromDate, ct);

    public async Task<List<VacationRequest>> SearchAsync(int? employeeId, RequestStatus? status, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
                          .Include(x => x.VacationType)
                          .Where(x => !x.IsDeleted);

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query.OrderByDescending(x => x.CreatedAt)
                          .ThenByDescending(x => x.Id)
                          .ToListAsync(ct);
    }

    public async Task<VacationRequest?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(x => x.VacationType)
                       .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
}
