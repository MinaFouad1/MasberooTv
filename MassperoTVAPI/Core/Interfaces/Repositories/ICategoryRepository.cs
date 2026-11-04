
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Categories;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(GetCategoriesQueryDto query);
    Task<CategoryDetailsDto?> GetCategorySummaryAsync(int id);
}
