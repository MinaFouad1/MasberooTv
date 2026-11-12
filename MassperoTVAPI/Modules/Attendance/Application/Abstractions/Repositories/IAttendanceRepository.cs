using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
    /// <summary>True when a non-deleted row exists for (employee, work date, shift). A null shiftId matches only rows without a shift.</summary>
    Task<bool> ExistsForEmployeeDateShiftAsync(int employeeId, DateOnly workDate, int? shiftId, CancellationToken ct = default);

    /// <summary>One non-deleted row as a DTO (permissions not filled).</summary>
    Task<AttendanceDto?> GetDtoByIdAsync(int id, CancellationToken ct = default);

    /// <summary>One page of the filtered rows, ordered WorkDate desc, employee name, CheckIn, Id (permissions not filled).</summary>
    Task<List<AttendanceDto>> SearchPageAsync(AttendanceSearchFilter filter, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>Statistics of ALL filtered rows in ONE aggregate query.</summary>
    Task<AttendanceStatisticsRow> GetStatisticsAsync(AttendanceSearchFilter filter, CancellationToken ct = default);
}
