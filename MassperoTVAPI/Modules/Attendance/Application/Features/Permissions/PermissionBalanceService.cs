using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public class PermissionBalanceService : IPermissionBalanceService
{
    private const int    MinYear                 = 2000;
    private const int    MaxYear                 = 2100;
    private const int    MaxItems                = 20;
    private const int    MaxAllowedMinutes       = 5000;
    private const string ConcurrencyErrorMessage = "Balance changed, please retry.";

    private readonly IUnitOfWork                _uow;
    private readonly IEmployeeLookup            _employeeLookup;
    private readonly IPermissionUsageCalculator _usageCalculator;
    private readonly IConflictAwareSaveChanges  _saveChanges;

    public PermissionBalanceService(IUnitOfWork uow, IEmployeeLookup employeeLookup,
        IPermissionUsageCalculator usageCalculator, IConflictAwareSaveChanges saveChanges)
    {
        _uow             = uow;
        _employeeLookup  = employeeLookup;
        _usageCalculator = usageCalculator;
        _saveChanges     = saveChanges;
    }

    public async Task<ApiResponse<List<PermissionBalanceDto>>> GetByEmployeeAsync(int employeeId, int? year, int? month, CancellationToken ct = default)
    {
        if (year is < MinYear or > MaxYear)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"Year must be between {MinYear} and {MaxYear}.");
        if (month is < 1 or > 12)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse("Month must be between 1 and 12.");

        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<List<PermissionBalanceDto>>.NotFoundResponse($"Employee {employeeId} not found.");

        return ApiResponse<List<PermissionBalanceDto>>.SuccessResponse(
            await _uow.PermissionBalances.GetByEmployeeAsync(employeeId, year, month, ct));
    }

    public async Task<ApiResponse<List<PermissionBalanceDto>>> CreateAsync(int employeeId, CreatePermissionBalancesDto dto, CancellationToken ct = default)
    {
        // 1. Employee
        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<List<PermissionBalanceDto>>.NotFoundResponse($"Employee {employeeId} not found.");

        // 2. Year
        if (dto.Year < MinYear || dto.Year > MaxYear)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"Year must be between {MinYear} and {MaxYear}.");

        // 3. Months (omitted = all 12 months)
        if (dto.Months is { Count: 0 })
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse("Months must contain at least one month when given (omit it for all 12 months).");
        var months = dto.Months ?? Enumerable.Range(1, 12).ToList();
        var badMonths = months.Where(m => m < 1 || m > 12).Distinct().ToList();
        if (badMonths.Count > 0)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"Months must be between 1 and 12: {string.Join(", ", badMonths)}.");
        var duplicateMonths = months.GroupBy(m => m).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateMonths.Count > 0)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"Duplicate month(s) in the request: {string.Join(", ", duplicateMonths)}.");

        // 4. Items
        if (dto.Items is null || dto.Items.Count == 0)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse("At least one item is required.");
        if (dto.Items.Count > MaxItems)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"At most {MaxItems} items are allowed.");
        var duplicateTypeIds = dto.Items.GroupBy(i => i.PermissionTypeId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateTypeIds.Count > 0)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse(
                $"Duplicate permission type(s) in the request: {string.Join(", ", duplicateTypeIds)}.");

        // 5. Permission types: exist, not deleted, active (one query)
        var typeIds  = dto.Items.Select(i => i.PermissionTypeId).ToList();
        var types    = (await _uow.PermissionTypes.GetByIdsNotDeletedAsync(typeIds, ct)).ToDictionary(t => t.Id);
        var badTypes = typeIds.Where(id => !types.TryGetValue(id, out var t) || !t.IsActive).ToList();
        if (badTypes.Count > 0)
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse(
                $"Permission type(s) not found, deleted or inactive: {string.Join(", ", badTypes)}.");

        // 6. Allowed minutes (required, no default)
        if (dto.Items.Any(i => i.AllowedMinutes is null))
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse("AllowedMinutes is required for every item.");
        if (dto.Items.Any(i => i.AllowedMinutes is < 0 or > MaxAllowedMinutes))
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse($"AllowedMinutes must be between 0 and {MaxAllowedMinutes}.");

        // 7. All or nothing: none of the (type, month) combinations may exist yet
        var existing = await _uow.PermissionBalances.GetExistingAsync(employeeId, dto.Year, typeIds, months, ct);
        if (existing.Count > 0)
            return ApiConflictResponse<List<PermissionBalanceDto>, PermissionBalanceConflictDto>.Create(
                $"Employee {employeeId} already has {existing.Count} of these permission balance(s). Use PUT to change them. Nothing was saved.",
                existing);

        // 8. One balance per (type x month), saved together
        var now = DateTimeOffset.UtcNow;
        var created = (from item in dto.Items
                       from month in months.OrderBy(m => m)
                       select new PermissionBalance
                       {
                           EmployeeId       = employeeId,
                           PermissionTypeId = item.PermissionTypeId,
                           Year             = dto.Year,
                           Month            = month,
                           AllowedMinutes   = item.AllowedMinutes!.Value,
                           UsedMinutes      = 0,
                           CreatedAt        = now
                       }).ToList();

        await _uow.PermissionBalances.AddRangeAsync(created, ct);
        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<List<PermissionBalanceDto>>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<List<PermissionBalanceDto>>.CreatedResponse(
            created.Select(b => b.ToDto(types[b.PermissionTypeId].Name))
                   .OrderBy(b => b.Year).ThenBy(b => b.Month).ThenBy(b => b.PermissionTypeName)
                   .ToList(),
            "Permission balances created successfully.");
    }

    public async Task<ApiResponse<PermissionBalanceDto>> UpdateAsync(int employeeId, UpdatePermissionBalanceDto dto, CancellationToken ct = default)
    {
        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<PermissionBalanceDto>.NotFoundResponse($"Employee {employeeId} not found.");

        if (dto.Year < MinYear || dto.Year > MaxYear)
            return ApiResponse<PermissionBalanceDto>.ErrorResponse($"Year must be between {MinYear} and {MaxYear}.");
        if (dto.Month < 1 || dto.Month > 12)
            return ApiResponse<PermissionBalanceDto>.ErrorResponse("Month must be between 1 and 12.");
        if (dto.AllowedMinutes is null)
            return ApiResponse<PermissionBalanceDto>.ErrorResponse("AllowedMinutes is required.");
        if (dto.AllowedMinutes is < 0 or > MaxAllowedMinutes)
            return ApiResponse<PermissionBalanceDto>.ErrorResponse($"AllowedMinutes must be between 0 and {MaxAllowedMinutes}.");

        var balance = await _uow.PermissionBalances.GetForUpdateAsync(employeeId, dto.PermissionTypeId, dto.Year, dto.Month, ct);
        if (balance is null)
            return ApiResponse<PermissionBalanceDto>.NotFoundResponse(
                $"No permission balance found for employee {employeeId}, permission type {dto.PermissionTypeId}, {dto.Year}-{dto.Month:00}.");

        // New allowance, then RECOMPUTE the whole group (lowering below used minutes is allowed)
        balance.AllowedMinutes = dto.AllowedMinutes.Value;
        var group = await _uow.PermissionRequests.GetApprovedGroupForUpdateAsync(employeeId, dto.PermissionTypeId, dto.Year, dto.Month, ct);
        PermissionUsageRecalculation.Apply(_usageCalculator, balance, group, DateTimeOffset.UtcNow);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<PermissionBalanceDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<PermissionBalanceDto>.SuccessResponse(balance.ToDto(balance.PermissionType.Name),
            "Permission balance updated successfully.");
    }
}
