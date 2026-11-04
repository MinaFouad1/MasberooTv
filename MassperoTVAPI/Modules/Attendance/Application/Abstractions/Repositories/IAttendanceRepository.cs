using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
}
