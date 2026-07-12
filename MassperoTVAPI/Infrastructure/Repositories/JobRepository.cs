using MassperoTV.Infrastructure.Repositories;
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

    public async Task<Job?> GetByIdWithCategoryAsync(int id)
        => await _context.Jobs
            .Include(j => j.Category)
            .Include(j => j.Location)
            .Include(j => j.HiringManager)
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
}

