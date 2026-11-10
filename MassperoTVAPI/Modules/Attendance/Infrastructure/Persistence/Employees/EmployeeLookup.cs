using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Modules.AttendanceModule.Infrastructure.Persistence.Employees;

/// <summary>Reads the existing Employees set directly (Employee has no IsDeleted column).</summary>
public class EmployeeLookup : IEmployeeLookup
{
    private readonly ApplicationDbContext _context;

    public EmployeeLookup(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int employeeId, CancellationToken ct = default)
        => await _context.Employees.AsNoTracking().AnyAsync(e => e.Id == employeeId, ct);

    public async Task<List<int>> GetExistingIdsAsync(IEnumerable<int> employeeIds, CancellationToken ct = default)
    {
        var ids = employeeIds.Distinct().ToList();
        if (ids.Count == 0)
            return new List<int>();

        return await _context.Employees.AsNoTracking()
                                       .Where(e => ids.Contains(e.Id))
                                       .Select(e => e.Id)
                                       .ToListAsync(ct);
    }
}