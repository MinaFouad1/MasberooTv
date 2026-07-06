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

    public async Task<(IEnumerable<Candidate> Items, int TotalCount)> GetPagedAsync(
        string?   candidateName,
        int?      jobId,
        int?      categoryId,
        int?      statusId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int       page,
        int       pageSize)
    {
        var query = _context.Candidates
            .Include(c => c.Job)
                .ThenInclude(j => j.Category)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .AsNoTracking()
            .AsQueryable();

        // ── Filters ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(candidateName))
            query = query.Where(c => c.Name.Contains(candidateName));

        if (jobId.HasValue)
            query = query.Where(c => c.JobId == jobId.Value);

        if (categoryId.HasValue)
            query = query.Where(c => c.Job.CategoryId == categoryId.Value);

        if (statusId.HasValue)
            query = query.Where(c => c.StatusId == statusId.Value);

        if (dateFrom.HasValue)
            query = query.Where(c => c.HiringDate.HasValue && c.HiringDate.Value.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(c => c.HiringDate.HasValue && c.HiringDate.Value.Date <= dateTo.Value.Date);

        // ── Pagination ────────────────────────────────────────────────────────
        var totalCount = await query.CountAsync();

        var safePage     = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Candidate?> GetByIdWithDetailsAsync(int id)
        => await _context.Candidates
            .Include(c => c.Job)
                .ThenInclude(j => j.Category)
            .Include(c => c.Status)
            .Include(c => c.SecurityClearance)
            .Include(c => c.User)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Candidate>> GetAllWithInterviewsAsync()
        => await _context.Candidates
            .Include(c => c.Status)
            .Include(c => c.Interviews)
                .ThenInclude(i => i.Type)
            .AsNoTracking()
            .ToListAsync();
}

