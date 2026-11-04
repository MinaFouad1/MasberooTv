
using MassperoTVAPI.Modules.Hr.Domain.Entities.Categories;
using MassperoTVAPI.Shared.Persistence;
using MassperoTVAPI.Modules.Hr.Application.Features.Common;

namespace MassperoTVAPI.Modules.Hr.Application.Abstractions.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(GetCategoriesQueryDto query);
    Task<CategoryDetailsDto?> GetCategorySummaryAsync(int id);
}
