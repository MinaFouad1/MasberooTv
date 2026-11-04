using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Attendances;

public class AttendancePunchRepository : GenericRepository<AttendancePunch>, IAttendancePunchRepository
{
    public AttendancePunchRepository(ApplicationDbContext context) : base(context) { }
}
