using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionRequestRepository : GenericRepository<PermissionRequest>, IPermissionRequestRepository
{
    public PermissionRequestRepository(ApplicationDbContext context) : base(context) { }
}
