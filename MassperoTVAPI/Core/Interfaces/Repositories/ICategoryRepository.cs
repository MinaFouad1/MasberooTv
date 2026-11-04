
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Categories;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(GetCategoriesQueryDto query);
    Task<CategoryDetailsDto?> GetCategorySummaryAsync(int id);
}
