using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IShiftAssignmentRepository : IGenericRepository<ShiftAssignment>
{
}
