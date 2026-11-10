using MassperoTVAPI.Shared.Application;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>
/// The project's <see cref="ApiResponse{T}"/> plus a typed list of conflicts,
/// used when a 409 has to tell the caller exactly which items clashed.
/// </summary>
public class ApiConflictResponse<T, TConflict> : ApiResponse<T>
{
    public List<TConflict> Conflicts { get; set; } = new();

    public static ApiConflictResponse<T, TConflict> Create(string message, List<TConflict> conflicts)
        => new() { Success = false, Message = message, StatusCode = 409, Conflicts = conflicts };
}
