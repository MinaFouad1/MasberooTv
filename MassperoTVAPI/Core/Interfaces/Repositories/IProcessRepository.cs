using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IProcessRepository : IGenericRepository<Process>
{
    /// <summary>Gets a process with all its dependencies, phase, status, and issues eagerly loaded.</summary>
    Task<Process?> GetWithDependenciesAsync(int id);

    /// <summary>Gets all processes with Phase, ProcessStatus, and Issues eagerly loaded.</summary>
    Task<IEnumerable<Process>> GetAllWithDetailsAsync();
}
