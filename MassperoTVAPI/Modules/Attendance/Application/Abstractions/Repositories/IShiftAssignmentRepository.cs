using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IShiftAssignmentRepository : IGenericRepository<ShiftAssignment>
{
    Task<List<DateOnly>> GetDayOffDatesAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    /// <summary>Non-deleted rows of the employees between the dates, TRACKED, with Shift and RosterMonth (one query).</summary>
    Task<List<ShiftAssignment>> GetForEmployeesInRangeForUpdateAsync(IReadOnlyCollection<int> employeeIds, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    /// <summary>RosterMonthId of every non-deleted row of the employee between the dates.</summary>
    Task<List<int>> GetRosterMonthIdsAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ShiftAssignment> rows, CancellationToken ct = default);
    Task<List<RosterScheduleRowDto>> GetScheduleAsync(DateOnly fromDate, DateOnly toDate, int? teamId, int? employeeId, CancellationToken ct = default);
}
