using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;

public class ShiftAssignmentRepository : GenericRepository<ShiftAssignment>, IShiftAssignmentRepository
{
    public ShiftAssignmentRepository(ApplicationDbContext context) : base(context) { }
    public async Task<List<DateOnly>> GetDayOffDatesAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted
                                && x.EmployeeId == employeeId
                                && x.DayType == DayType.DayOff
                                && x.WorkDate >= fromDate
                                && x.WorkDate <= toDate)
                       .Select(x => x.WorkDate)
                       .Distinct()
                       .ToListAsync(ct);

    public async Task<List<ShiftAssignment>> GetForEmployeesInRangeForUpdateAsync(IReadOnlyCollection<int> employeeIds, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        var idList = employeeIds.Distinct().ToList();
        return await _dbSet.Include(x => x.Shift)
                           .Include(x => x.RosterMonth)
                           .Where(x => !x.IsDeleted
                                    && idList.Contains(x.EmployeeId)
                                    && x.WorkDate >= fromDate
                                    && x.WorkDate <= toDate)
                           .ToListAsync(ct);
    }

    public async Task<List<int>> GetRosterMonthIdsAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
                       .Where(x => !x.IsDeleted
                                && x.EmployeeId == employeeId
                                && x.WorkDate >= fromDate
                                && x.WorkDate <= toDate)
                       .Select(x => x.RosterMonthId)
                       .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<ShiftAssignment> rows, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(rows, ct);

    public async Task<List<RosterScheduleRowDto>> GetScheduleAsync(DateOnly fromDate, DateOnly toDate, int? teamId, int? employeeId, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
                          .Where(x => !x.IsDeleted && x.WorkDate >= fromDate && x.WorkDate <= toDate);
        if (teamId is int team)
            query = query.Where(x => x.Employee.TeamId == team);
        if (employeeId is int employee)
            query = query.Where(x => x.EmployeeId == employee);

        // Only the employee Id and display name are read from HR.
        return await query
            .OrderBy(x => x.Employee.Name).ThenBy(x => x.EmployeeId)
            .ThenBy(x => x.WorkDate)
            .ThenBy(x => x.Shift == null ? (TimeOnly?)null : x.Shift.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new RosterScheduleRowDto(
                x.Id,
                x.EmployeeId,
                x.Employee.Name,
                x.WorkDate,
                x.DayType,
                x.ShiftId,
                x.Shift == null ? null : x.Shift.Name,
                x.Shift == null ? null : (TimeOnly?)x.Shift.StartTime,
                x.Shift == null ? null : (int?)x.Shift.DurationMinutes,
                x.Note))
            .ToListAsync(ct);
    }
}
