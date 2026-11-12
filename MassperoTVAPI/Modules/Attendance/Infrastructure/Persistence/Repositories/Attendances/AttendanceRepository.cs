using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Persistence;
using System.Linq.Expressions;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Attendances;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context) { }

    private static readonly Expression<Func<Attendance, AttendanceDto>> ToDto = x => new AttendanceDto
    {
        Id                = x.Id,
        EmployeeId        = x.EmployeeId,
        EmployeeName      = x.Employee.Name,
        JobId             = x.Employee.JobId,
        JobName           = x.Employee.Job != null ? x.Employee.Job.Name : null,
        WorkDate          = x.WorkDate,
        ShiftId           = x.ShiftId,
        ShiftName         = x.Shift != null ? x.Shift.Name : null,
        TimeZoneId        = x.Shift != null ? x.Shift.TimeZoneId : null,
        ExpectedStart     = x.ExpectedStart,
        ExpectedEnd       = x.ExpectedEnd,
        CheckIn           = x.CheckIn,
        CheckOut          = x.CheckOut,
        LateMinutes       = x.LateMinutes,
        EarlyLeaveMinutes = x.EarlyLeaveMinutes,
        WorkedMinutes     = x.WorkedMinutes,
        Status            = x.Status,
        IsManualOverride  = x.IsManualOverride
    };

    public async Task<bool> ExistsForEmployeeDateShiftAsync(int employeeId, DateOnly workDate, int? shiftId, CancellationToken ct = default)
        => shiftId is null
            ? await _dbSet.AnyAsync(x => !x.IsDeleted && x.EmployeeId == employeeId && x.WorkDate == workDate && x.ShiftId == null, ct)
            : await _dbSet.AnyAsync(x => !x.IsDeleted && x.EmployeeId == employeeId && x.WorkDate == workDate && x.ShiftId == shiftId, ct);

    public async Task<AttendanceDto?> GetDtoByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => x.Id == id && !x.IsDeleted)
                       .Select(ToDto)
                       .FirstOrDefaultAsync(ct);

    public async Task<List<AttendanceDto>> SearchPageAsync(AttendanceSearchFilter filter, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var skip = (int)Math.Min((long)(pageNumber - 1) * pageSize, int.MaxValue);
        return await ApplyFilter(filter)
                     .OrderByDescending(x => x.WorkDate)
                     .ThenBy(x => x.Employee.Name)
                     .ThenBy(x => x.CheckIn)
                     .ThenBy(x => x.Id)
                     .Skip(skip)
                     .Take(pageSize)
                     .Select(ToDto)
                     .ToListAsync(ct);
    }

    public async Task<AttendanceStatisticsRow> GetStatisticsAsync(AttendanceSearchFilter filter, CancellationToken ct = default)
        => await StatisticsQuery(ApplyFilter(filter)).FirstOrDefaultAsync(ct)
           ?? new AttendanceStatisticsRow(0, null, 0);

    /// <summary>
    /// ONE aggregate SELECT over the filtered rows (GROUP BY a constant): COUNT(*), AVG(CASE WHEN LateMinutes &gt; 0 ...) and
    /// COUNT(CASE WHEN Status = 'Absent' ...). AVG ignores NULL, so it averages the late rows only (NULL when none).
    /// No row matches: no group, the caller gets zeros.
    /// </summary>
    private static IQueryable<AttendanceStatisticsRow> StatisticsQuery(IQueryable<Attendance> query)
        => query.GroupBy(_ => 1)
                .Select(g => new AttendanceStatisticsRow(
                    g.Count(),
                    g.Average(x => x.LateMinutes > 0 ? (double?)x.LateMinutes : null),
                    g.Count(x => x.Status == AttendanceStatus.Absent)));

    /// <summary>Non-deleted rows matching every given filter (AND). No global query filters exist, so IsDeleted is explicit.</summary>
    private IQueryable<Attendance> ApplyFilter(AttendanceSearchFilter filter)
    {
        var query = _dbSet.AsNoTracking().Where(x => !x.IsDeleted);

        if (filter.StartDate is not null)
        {
            var startDate = filter.StartDate.Value;
            query = query.Where(x => x.WorkDate >= startDate);
        }
        if (filter.EndDate is not null)
        {
            var endDate = filter.EndDate.Value;
            query = query.Where(x => x.WorkDate <= endDate);
        }
        if (filter.ShiftId is not null)
        {
            var shiftId = filter.ShiftId.Value;
            query = query.Where(x => x.ShiftId == shiftId);
        }
        if (filter.EmployeeId is not null)
        {
            var employeeId = filter.EmployeeId.Value;
            query = query.Where(x => x.EmployeeId == employeeId);
        }
        if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
        {
            var term = filter.EmployeeName.Trim().ToLower();
            query = query.Where(x => x.Employee.Name.ToLower().Contains(term));
        }
        if (filter.JobId is not null)
        {
            var jobId = filter.JobId.Value;
            query = query.Where(x => x.Employee.JobId == jobId);
        }
        if (filter.Status is not null)
        {
            var status = filter.Status.Value;
            query = query.Where(x => x.Status == status);
        }
        if (filter.PermissionTypeId is not null)
        {
            var permissionTypeId = filter.PermissionTypeId.Value;
            var permissions      = _context.Set<PermissionRequest>();
            query = query.Where(x => permissions.Any(p => !p.IsDeleted
                                                       && p.Status == RequestStatus.Approved
                                                       && p.EmployeeId == x.EmployeeId
                                                       && p.PermissionDate == x.WorkDate
                                                       && p.PermissionTypeId == permissionTypeId));
        }

        return query;
    }
}
