using MassperoTVAPI.Core.Entities;

using MassperoTVAPI.Core.DTOs;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(GetCategoriesQueryDto query);
    Task<CategoryDetailsDto?> GetCategorySummaryAsync(int id);
}
