using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Core.DTOs;

namespace MassperoTVAPI.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<Category>> GetPagedAsync(GetCategoriesQueryDto query)
    {
        var dbQuery = _context.Categories.Include(c => c.Jobs).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.CategoryName))
        {
            dbQuery = dbQuery.Where(c => c.Name.Contains(query.CategoryName));
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.IsActive == query.IsActive.Value);
        }

        var totalCount = await dbQuery.CountAsync();
        
        var items = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<Category>(
            items,
            totalCount,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalCount / (double)query.PageSize)
        );
    }

    public async Task<CategoryDetailsDto?> GetCategorySummaryAsync(int id)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return null;

        var jobsQuery = _context.Jobs
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Status)
            .Include(j => j.Candidates)
                .ThenInclude(c => c.Interviews)
            .Where(j => j.CategoryId == id)
            .AsNoTracking()
            .AsQueryable();

       
        var jobs = await jobsQuery.ToListAsync();

        var openPositions = jobs.Sum(j => j.OpenPositions);
        var applications = jobs.SelectMany(j => j.Candidates)
            .Count(c => !string.Equals(c.Status?.Name, "Hired", StringComparison.OrdinalIgnoreCase));
        var interviews = jobs.SelectMany(j => j.Candidates)
            .SelectMany(c => c.Interviews)
            .Count();
        var offersSent = jobs.SelectMany(j => j.Candidates)
            .Count(c => string.Equals(c.Status?.Name, "Offered", StringComparison.OrdinalIgnoreCase));

        return new CategoryDetailsDto(
            category.Id,
            category.Name,
            category.CategoryCode,
            category.Description,
            category.IsActive,
            category.IsActive ? "Active" : "Inactive",
            category.CreatedAt,
            openPositions,
            applications,
            interviews,
            offersSent
        );
    }
}
