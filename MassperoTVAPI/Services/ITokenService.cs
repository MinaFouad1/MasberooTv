using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;

namespace MassperoTVAPI.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}
