using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Core.Interfaces.Repositories;

public interface IConfigurationRepository : IGenericRepository<AppConfiguration>
{
    /// <summary>Find a configuration entry by its Key (case-insensitive).</summary>
    Task<AppConfiguration?> GetByKeyAsync(string key);

    ///// <summary>Check whether a key already exists.</summary>
    Task<bool> KeyExistsAsync(string key);
}
