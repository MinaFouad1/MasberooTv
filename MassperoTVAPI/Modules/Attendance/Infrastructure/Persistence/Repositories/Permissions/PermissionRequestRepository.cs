using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionRequestRepository : GenericRepository<PermissionRequest>, IPermissionRequestRepository
{
    public PermissionRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<AttendanceDayPermissionDto>> GetApprovedForEmployeesInRangeAsync(IReadOnlyCollection<int> employeeIds, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        var idList = employeeIds.Distinct().ToList();
        if (idList.Count == 0)
            return new List<AttendanceDayPermissionDto>();

        return await _dbSet.AsNoTracking()
                           .Where(x => !x.IsDeleted
                                    && x.Status == RequestStatus.Approved
                                    && idList.Contains(x.EmployeeId)
                                    && x.PermissionDate >= fromDate
                                    && x.PermissionDate <= toDate)
                           .OrderBy(x => x.PermissionDate)
                           .ThenBy(x => x.Id)
                           .Select(x => new AttendanceDayPermissionDto(
                               x.EmployeeId,
                               x.PermissionDate,
                               x.Id,
                               x.PermissionTypeId,
                               x.PermissionType.Name,
                               x.Minutes,
                               x.ExceededMinutes))
                           .ToListAsync(ct);
    }

    /// <summary>Approved, non-deleted requests of one (employee, type, month), TRACKED, ordered by PermissionDate then Id.</summary>
    public async Task<List<PermissionRequest>> GetApprovedGroupForUpdateAsync(int employeeId, int permissionTypeId, int year, int month, CancellationToken ct = default)
    {
        var fromDate = new DateOnly(year, month, 1);
        var toDate   = fromDate.AddMonths(1).AddDays(-1);
        return await _dbSet.Where(x => !x.IsDeleted
                                    && x.Status == RequestStatus.Approved
                                    && x.EmployeeId == employeeId
                                    && x.PermissionTypeId == permissionTypeId
                                    && x.PermissionDate >= fromDate
                                    && x.PermissionDate <= toDate)
                           .OrderBy(x => x.PermissionDate).ThenBy(x => x.Id)
                           .ToListAsync(ct);
    }

    public async Task<PermissionRequest?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default)
        => await _dbSet.Include(x => x.PermissionType)
                       .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    /// <summary>One query: requests + type name + that month's balance (correlated, null when missing).</summary>
    public async Task<List<PermissionRequestDto>> SearchAsync(int? employeeId, int? permissionTypeId, RequestStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().Where(x => !x.IsDeleted);
        if (employeeId is int employee)
            query = query.Where(x => x.EmployeeId == employee);
        if (permissionTypeId is int typeId)
            query = query.Where(x => x.PermissionTypeId == typeId);
        if (status is RequestStatus requestStatus)
            query = query.Where(x => x.Status == requestStatus);
        if (from is DateOnly fromDate)
            query = query.Where(x => x.PermissionDate >= fromDate);
        if (to is DateOnly toDate)
            query = query.Where(x => x.PermissionDate <= toDate);

        return await query
            .OrderByDescending(x => x.PermissionDate).ThenByDescending(x => x.Id)
            .Select(x => new PermissionRequestDto(
                x.Id,
                x.EmployeeId,
                x.PermissionTypeId,
                x.PermissionType.Name,
                x.PermissionDate,
                x.Minutes,
                x.ExceededMinutes,
                x.ExceededMinutes > 0,
                x.Status,
                x.Reason,
                x.CreatedAt,
                _context.PermissionBalances
                        .Where(b => !b.IsDeleted
                                 && b.EmployeeId == x.EmployeeId
                                 && b.PermissionTypeId == x.PermissionTypeId
                                 && b.Year == x.PermissionDate.Year
                                 && b.Month == x.PermissionDate.Month)
                        .Select(b => (int?)b.UsedMinutes)
                        .FirstOrDefault(),
                _context.PermissionBalances
                        .Where(b => !b.IsDeleted
                                 && b.EmployeeId == x.EmployeeId
                                 && b.PermissionTypeId == x.PermissionTypeId
                                 && b.Year == x.PermissionDate.Year
                                 && b.Month == x.PermissionDate.Month)
                        .Select(b => (int?)b.AllowedMinutes)
                        .FirstOrDefault()))
            .ToListAsync(ct);
    }
}
