using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;

public interface IPermissionTypeRepository : IGenericRepository<PermissionType>
{
}
