using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    public JobRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Job>> GetAllWithCategoryAsync(string? search = null, int? categoryId = null, DateTime? date = null, int? locationId = null, string? hiringManagerId = null)
    {
        var query = _context.Jobs
            .Include(j => j.Category)
            .Include(j => j.Location)
            .Include(j => j.HiringManager)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Status)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(j => j.Name.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(j => j.CategoryId == categoryId.Value);

        if (date.HasValue)
            query = query.Where(j => j.CreatedAt.Date >= date.Value.Date);

        if (locationId.HasValue)
            query = query.Where(j => j.LocationId == locationId.Value);

        if (!string.IsNullOrWhiteSpace(hiringManagerId))
            query = query.Where(j => j.HiringManagerId == hiringManagerId);

        return await query.ToListAsync();
    }

    public async Task<PagedResult<Job>> GetPagedAsync(GetJobsQueryDto query)
    {
        var dbQuery = _context.Jobs
            .Include(j => j.Category)
            .Include(j => j.Location)
            .Include(j => j.HiringManager)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            dbQuery = dbQuery.Where(j => j.Name.Contains(query.Search));

        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(j => j.CategoryId == query.CategoryId.Value);

        if (query.Date.HasValue)
            dbQuery = dbQuery.Where(j => j.CreatedAt.Date >= query.Date.Value.Date);

        if (query.LocationId.HasValue)
            dbQuery = dbQuery.Where(j => j.LocationId == query.LocationId.Value);

        if (!string.IsNullOrWhiteSpace(query.HiringManagerId))
            dbQuery = dbQuery.Where(j => j.HiringManagerId == query.HiringManagerId);

        if (query.IsOpend.HasValue)
            dbQuery = dbQuery.Where(j => j.IsOpend == query.IsOpend.Value);

        var totalCount = await dbQuery.CountAsync();

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await dbQuery
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Job>(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<Job?> GetByIdWithCategoryAsync(int id)
        => await _context.Jobs
            .Include(j => j.Category)
            .Include(j => j.Location)
            .Include(j => j.HiringManager)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Status)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<IEnumerable<Job>> GetAllWithCandidatesAsync()
        => await _context.Jobs
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Status)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .Include(j => j.HiringManager)
            .AsNoTracking()
            .ToListAsync();

    public async Task<JobRecruitmentProgressDto?> GetRecruitmentProgressAsync(int? id)
    {
        if (id == null || id ==0)
        {
            return null;
        }

        var job = await _context.Jobs
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Status)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
                    .ThenInclude(i => i.Type)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
            return null;

        var applications = job.Candidates.Count(c =>
            !string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase));

        var hrInterviews = job.Candidates.Count(c =>
            c.Interviews.Any(i => string.Equals(i.Type?.Name, "HR", StringComparison.OrdinalIgnoreCase) ||
                                  (i.Type != null && i.Type.Name.Contains("HR", StringComparison.OrdinalIgnoreCase))));

        var technicalInterviews = job.Candidates.Count(c =>
            c.Interviews.Any(i => string.Equals(i.Type?.Name, "Technical", StringComparison.OrdinalIgnoreCase) ||
                                  (i.Type != null && i.Type.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase))));

        var offersSent = job.Candidates.Count(c =>
            string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase));

        var hired = job.Candidates.Count(c =>
            string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase));

        var accepted = job.Candidates.Count(c =>
            c.Accepted == true ||
            string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status?.Name, "SignContract", StringComparison.OrdinalIgnoreCase));

        return new JobRecruitmentProgressDto(
            JobId: job.Id,
            JobName: job.Name,
            Applications: applications,
            HrInterviews: hrInterviews,
            TechnicalInterviews: technicalInterviews,
            OffersSent: offersSent,
            Accepted: accepted,
            Hired: hired,
            TotalApplications: job.Candidates.Count
        );
    }
}

