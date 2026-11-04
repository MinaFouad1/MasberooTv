
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassperoTVAPI.Modules.Hr.Domain.Entities.Configurations;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.Hr.Infrastructure.Persistence.Repositories.Configurations;

public class ConfigurationRepository : GenericRepository<AppConfiguration>, IConfigurationRepository
{
    public ConfigurationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AppConfiguration?> GetByKeyAsync(string key)
        => await _context.Configurations
            .FirstOrDefaultAsync(c => c.Key.ToLower() == key.ToLower());

    public async Task<bool> KeyExistsAsync(string key)
        => await _context.Configurations
            .AnyAsync(c => c.Key.ToLower() == key.ToLower());
}
