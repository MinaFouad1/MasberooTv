using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;

public class RosterMonthRepository : GenericRepository<RosterMonth>, IRosterMonthRepository
{
    public RosterMonthRepository(ApplicationDbContext context) : base(context) { }
}
