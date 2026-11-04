using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Shifts;

public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext context) : base(context) { }
}
