using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IIssueRepository : IGenericRepository<Issue>
{
    Task<IEnumerable<Issue>> GetAllWithDetailsAsync();
    Task<IEnumerable<Issue>> GetAllWithDetailsAsync(int? processId, DateTime? date, bool? resolved);
    Task<Issue?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Issue>> GetByProcessAsync(int processId);
}
