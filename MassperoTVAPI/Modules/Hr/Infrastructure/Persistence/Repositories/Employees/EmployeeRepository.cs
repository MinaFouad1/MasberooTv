using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.Hr.Application.Abstractions.Repositories;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Repositories.Employees
{
    public class EmployeeRepository:GenericRepository<Employee>,IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context ) :base(context)
        {
            
        }
    }
}
