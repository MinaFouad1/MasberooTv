using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Services;

public class ConfigService : IConfigService
{
    private readonly ApplicationDbContext _context;

    public ConfigService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetPathValueAsync(string key)
    {
        return await _context.Configurations
            .Where(c => c.Key.ToLower() == key.ToLower())
            .Select(c => c.Value)
            .FirstOrDefaultAsync();
    }
}
