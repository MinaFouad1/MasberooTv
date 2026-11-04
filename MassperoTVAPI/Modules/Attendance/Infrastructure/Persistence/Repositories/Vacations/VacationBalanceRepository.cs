using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;

public class VacationBalanceRepository : GenericRepository<VacationBalance>, IVacationBalanceRepository
{
    public VacationBalanceRepository(ApplicationDbContext context) : base(context) { }
}
