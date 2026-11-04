using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IHolidayRepository : IGenericRepository<Holiday>
{
}
