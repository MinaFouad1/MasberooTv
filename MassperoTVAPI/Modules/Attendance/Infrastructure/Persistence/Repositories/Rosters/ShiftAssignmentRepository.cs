using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Repositories.Rosters;

public class ShiftAssignmentRepository : GenericRepository<ShiftAssignment>, IShiftAssignmentRepository
{
    public ShiftAssignmentRepository(ApplicationDbContext context) : base(context) { }
}
