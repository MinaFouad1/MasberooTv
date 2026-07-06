namespace MassperoTVAPI.Core.Interfaces.Repositories;

/// <summary>
/// Generic read-write repository for simple lookup tables (no FK navigation needed).
/// </summary>
public interface ILookupRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
}
