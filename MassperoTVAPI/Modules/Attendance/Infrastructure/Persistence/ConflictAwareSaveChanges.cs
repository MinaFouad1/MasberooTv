using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Shared.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence;

/// <summary>
/// Saves through the existing UnitOfWork and translates EF Core / SQL Server conflicts
/// (RowVersion mismatch, unique index 2601 / unique constraint 2627) into
/// <see cref="ConcurrencyConflictException"/>, so the Application layer stays free of EF Core.
/// </summary>
public class ConflictAwareSaveChanges : IConflictAwareSaveChanges
{
    private const int UniqueIndexViolation      = 2601;
    private const int UniqueConstraintViolation = 2627;

    private readonly IUnitOfWork _uow;

    public ConflictAwareSaveChanges(IUnitOfWork uow)
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
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new ConcurrencyConflictException("Another request saved the same unique key first.", ex);
        }
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is SqlException sql && (sql.Number == UniqueIndexViolation || sql.Number == UniqueConstraintViolation))
                return true;
        }
        return false;
    }
}
