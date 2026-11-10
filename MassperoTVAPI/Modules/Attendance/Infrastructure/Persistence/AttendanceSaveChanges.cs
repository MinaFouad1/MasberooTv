using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence;

/// <summary>
/// Saves through the existing UnitOfWork and translates EF Core's concurrency error
/// into the Application-level <see cref="ConcurrencyConflictException"/>.
/// </summary>
public class AttendanceSaveChanges : IAttendanceSaveChanges
{
    private readonly IUnitOfWork _uow;

    public AttendanceSaveChanges(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // The existing IUnitOfWork.SaveChangesAsync has no CancellationToken parameter.
        ct.ThrowIfCancellationRequested();

        try
        {
            return await _uow.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The data was changed by another request.", ex);
        }
    }
}