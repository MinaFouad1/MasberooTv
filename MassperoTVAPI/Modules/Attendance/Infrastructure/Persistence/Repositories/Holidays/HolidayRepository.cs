using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Holidays;

public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
{
    public HolidayRepository(ApplicationDbContext context) : base(context) { }
}
