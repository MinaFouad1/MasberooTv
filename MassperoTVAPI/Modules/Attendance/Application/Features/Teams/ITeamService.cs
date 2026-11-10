using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Teams;

public interface ITeamService
{
    Task<ApiResponse<List<TeamDto>>> GetAllAsync(string? name, CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> UpdateAsync(int id, UpdateTeamDto dto, CancellationToken ct = default);
}
