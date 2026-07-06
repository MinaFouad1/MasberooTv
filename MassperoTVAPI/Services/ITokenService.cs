using MassperoTVAPI.Core.Entities;

namespace MassperoTVAPI.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}
