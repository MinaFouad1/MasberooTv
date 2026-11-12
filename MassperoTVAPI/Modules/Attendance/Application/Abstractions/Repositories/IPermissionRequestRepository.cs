using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IPermissionRequestRepository : IGenericRepository<PermissionRequest>
{
    Task<List<PermissionRequest>> GetApprovedGroupForUpdateAsync(int employeeId, int permissionTypeId, int year, int month, CancellationToken ct = default);
    Task<PermissionRequest?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
    Task<List<PermissionRequestDto>> SearchAsync(int? employeeId, int? permissionTypeId, RequestStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken ct = default);
    /// <summary>Approved, non-deleted requests of the employees between the dates (inclusive), in ONE query, with the type name.</summary>
    Task<List<AttendanceDayPermissionDto>> GetApprovedForEmployeesInRangeAsync(IReadOnlyCollection<int> employeeIds, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
}
