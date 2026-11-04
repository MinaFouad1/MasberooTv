using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Vacations;

public class VacationTypeRepository : GenericRepository<VacationType>, IVacationTypeRepository
{
    public VacationTypeRepository(ApplicationDbContext context) : base(context) { }
}
