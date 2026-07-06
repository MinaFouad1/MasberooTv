
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassperoTV.Infrastructure.Repositories;

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
