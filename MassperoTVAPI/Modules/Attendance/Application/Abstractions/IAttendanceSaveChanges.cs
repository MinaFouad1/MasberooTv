namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>
/// Saves all pending changes in one call (one database transaction).
/// Throws <see cref="ConcurrencyConflictException"/> when a concurrency check fails.
/// </summary>
public interface IAttendanceSaveChanges
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}