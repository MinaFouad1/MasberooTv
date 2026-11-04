using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionBalanceRepository : GenericRepository<PermissionBalance>, IPermissionBalanceRepository
{
    public PermissionBalanceRepository(ApplicationDbContext context) : base(context) { }
}
