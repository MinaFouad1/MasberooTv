using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.Hr.Application.Abstractions.Repositories;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Repositories.Interviews;

public class InterviewRepository : GenericRepository<Interview>, IInterviewRepository
{
    public InterviewRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Interview>> GetAllWithDetailsAsync()
        => await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.Type)
            .Include(i => i.Evaluator)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Interview?> GetByIdWithDetailsAsync(int id)
        => await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.Type)
            .Include(i => i.Evaluator)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Interview>> GetByCandidateAsync(int candidateId)
        => await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.Type)
            .Include(i => i.Evaluator)
            .Where(i => i.CandidateId == candidateId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Interview>> GetByCandidateAndTypeAsync(int candidateId, int typeId)
        => await _context.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.Type)
            .Include(i => i.Evaluator)
            .Where(i => i.CandidateId == candidateId && i.TypeId == typeId)
            .AsNoTracking()
            .ToListAsync();
}
