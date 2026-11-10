namespace MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;

/// <summary>
/// Saves all pending changes through the UnitOfWork in ONE call (one database transaction).
/// Throws <see cref="ConcurrencyConflictException"/> when a RowVersion check fails
/// or when a unique index is violated (another request saved the same key first).
/// </summary>
public interface IConflictAwareSaveChanges
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
