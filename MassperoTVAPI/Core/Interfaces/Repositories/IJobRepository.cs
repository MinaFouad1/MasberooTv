using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Jobs;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IJobRepository : IGenericRepository<Job>
{
    Task<IEnumerable<Job>> GetAllWithCategoryAsync(string? search = null, int? categoryId = null, DateTime? date = null, int? locationId = null, string? hiringManagerId = null);
    Task<PagedResult<Job>> GetPagedAsync(GetJobsQueryDto query);
    Task<Job?>             GetByIdWithCategoryAsync(int id);
    Task<IEnumerable<Job>> GetAllWithCandidatesAsync();
    Task<JobRecruitmentProgressDto?> GetRecruitmentProgressAsync(int? id);
}
