using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IInterviewRepository : IGenericRepository<Interview>
{
    Task<IEnumerable<Interview>> GetAllWithDetailsAsync();
    Task<Interview?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Interview>> GetByCandidateAsync(int candidateId);
    Task<IEnumerable<Interview>> GetByCandidateAndTypeAsync(int candidateId, int typeId);
}
