using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
{
    public CandidateRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Candidate>> GetAllAsync(string? name, int? statusId, int? jobId)
    {
        var query = _context.Candidates
            .Include(c => c.Job)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (statusId.HasValue)
            query = query.Where(c => c.StatusId == statusId.Value);

        if (jobId.HasValue)
            query = query.Where(c => c.JobId == jobId.Value);

        return await query.ToListAsync();
    }

    public async Task<Candidate?> GetByIdWithDetailsAsync(int id)
        => await _context.Candidates
            .Include(c => c.Job)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Candidate>> GetAllWithInterviewsAsync()
        => await _context.Candidates
            .Include(c => c.Status)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .AsNoTracking()
            .ToListAsync();
}

