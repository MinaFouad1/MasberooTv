using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
}
