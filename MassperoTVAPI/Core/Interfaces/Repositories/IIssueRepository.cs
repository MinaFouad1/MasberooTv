using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IIssueRepository : IGenericRepository<Issue>
{
    Task<IEnumerable<Issue>> GetAllWithDetailsAsync();
    Task<IEnumerable<Issue>> GetAllWithDetailsAsync(int? processId, DateTime? date, IssueStatus? status, string? name = null, IssuePriority? priority = null);
    Task<Issue?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Issue>> GetByProcessAsync(int processId);
    Task<IEnumerable<Issue>> GetTopRisksAsync(int count = 5);
}
