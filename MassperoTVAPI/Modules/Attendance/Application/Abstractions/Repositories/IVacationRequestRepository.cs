using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IVacationRequestRepository : IGenericRepository<VacationRequest>
{
    Task<bool> HasApprovedOverlapAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
    Task<List<VacationRequest>> SearchAsync(int? employeeId, RequestStatus? status, CancellationToken ct = default);
    Task<VacationRequest?> GetByIdNotDeletedAsync(int id, CancellationToken ct = default);
}
