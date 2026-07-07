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
}
