using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class IssueRepository : GenericRepository<Issue>, IIssueRepository
{
    public IssueRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Issue>> GetAllWithDetailsAsync()
        => await _context.Issues
            .Include(i => i.Process)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Issue>> GetAllWithDetailsAsync(int? processId, DateTime? date, bool? resolved, string? name = null, IssuePriority? priority = null)
    {
        var query = _context.Issues
            .Include(i => i.Process)
            .AsNoTracking()
            .AsQueryable();

        if (processId.HasValue)
            query = query.Where(i => i.ProcessId == processId.Value);

        if (date.HasValue)
        {
            var searchDate = date.Value.Date;
            query = query.Where(i => i.Date.HasValue && i.Date.Value.Date == searchDate);
        }

        if (resolved.HasValue)
            query = query.Where(i => i.Resolved == resolved.Value);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(i => i.Name.Contains(name));

        if (priority.HasValue)
            query = query.Where(i => i.Priority == priority.Value);

        return await query.ToListAsync();
    }

    public async Task<Issue?> GetByIdWithDetailsAsync(int id)
        => await _context.Issues
            .Include(i => i.Process)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Issue>> GetByProcessAsync(int processId)
        => await _context.Issues
            .Include(i => i.Process)
            .Where(i => i.ProcessId == processId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Issue>> GetTopRisksAsync(int count = 5)
    {
        return await _context.Issues
            .Include(i => i.Process)
            .Where(i => i.Resolved != true)
            .OrderByDescending(i => i.Priority)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }
}
