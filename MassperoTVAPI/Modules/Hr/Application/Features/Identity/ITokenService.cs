using MassperoTVAPI.Modules.Hr.Domain.Entities.Identity;

namespace MassperoTVAPI.Modules.Hr.Application.Features.Identity;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}
