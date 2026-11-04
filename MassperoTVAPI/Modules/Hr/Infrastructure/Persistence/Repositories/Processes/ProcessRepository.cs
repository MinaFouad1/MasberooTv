using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Processes;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Repositories.Processes;

public class ProcessRepository : GenericRepository<Process>, IProcessRepository
{
    public ProcessRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Process?> GetWithDependenciesAsync(int id)
        => await _context.Processes
            .Include(p => p.Phase)
            .Include(p => p.ProcessStatus)
            .Include(p => p.Issues)
            .Include(p => p.Dependencies)
                .ThenInclude(d => d.DependsOn)
            .Include(p => p.DependentProcesses)
                .ThenInclude(d => d.Process)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Process>> GetAllWithDetailsAsync()
        => await _context.Processes
            .Include(p => p.Phase)
            .Include(p => p.ProcessStatus)
            .Include(p => p.Issues)
            .Include(p => p.Dependencies)
                .ThenInclude(d => d.DependsOn)
            .AsNoTracking()
            .ToListAsync();
}
