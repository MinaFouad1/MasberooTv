using MassperoTVAPI.Modules.Hr.Domain.Entities.Interviews;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.Hr.Application.Abstractions.Repositories;

public interface IInterviewRepository : IGenericRepository<Interview>
{
    Task<IEnumerable<Interview>> GetAllWithDetailsAsync();
    Task<Interview?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Interview>> GetByCandidateAsync(int candidateId);
    Task<IEnumerable<Interview>> GetByCandidateAndTypeAsync(int candidateId, int typeId);
}
