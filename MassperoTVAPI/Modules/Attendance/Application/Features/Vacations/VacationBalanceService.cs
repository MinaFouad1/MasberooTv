using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public class VacationBalanceService : IVacationBalanceService
{
    private const int     MinYear  = 2000;
    private const int     MaxYear  = 2100;
    private const decimal MaxDays  = 999.99m; // column precision (5,2)

    private readonly IUnitOfWork _uow;
    private readonly IEmployeeLookup _employeeLookup;

    public VacationBalanceService(IUnitOfWork uow, IEmployeeLookup employeeLookup)
    {
        _uow            = uow;
        _employeeLookup = employeeLookup;
    }

    public async Task<ApiResponse<List<VacationBalanceDto>>> GetByEmployeeAsync(int employeeId, int? year, CancellationToken ct = default)
    {
        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<List<VacationBalanceDto>>.NotFoundResponse($"Employee {employeeId} not found.");

        var balances = await _uow.VacationBalances.GetByEmployeeAsync(employeeId, year, ct);
        return ApiResponse<List<VacationBalanceDto>>.SuccessResponse(
            balances.Select(b => b.ToDto(b.VacationType.Name)).ToList());
    }

    public async Task<ApiResponse<List<VacationBalanceDto>>> CreateAsync(int employeeId, CreateVacationBalancesDto dto, CancellationToken ct = default)
    {
        // 1. Request shape
        if (dto.Year < MinYear || dto.Year > MaxYear)
            return ApiResponse<List<VacationBalanceDto>>.ErrorResponse($"Year must be between {MinYear} and {MaxYear}.");

        if (dto.Items is null || dto.Items.Count == 0)
            return ApiResponse<List<VacationBalanceDto>>.ErrorResponse("At least one item is required.");

        foreach (var item in dto.Items)
        {
            if (item.Entitled is < 0 || item.CarriedOver is < 0)
                return ApiResponse<List<VacationBalanceDto>>.ErrorResponse("Entitled and CarriedOver must be 0 or more.");
            if (item.Entitled > MaxDays || item.CarriedOver > MaxDays)
                return ApiResponse<List<VacationBalanceDto>>.ErrorResponse($"Entitled and CarriedOver must be at most {MaxDays}.");
        }

        var duplicateTypeIds = dto.Items.GroupBy(i => i.VacationTypeId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateTypeIds.Count > 0)
            return ApiResponse<List<VacationBalanceDto>>.ErrorResponse(
                $"Duplicate vacation type(s) in the request: {string.Join(", ", duplicateTypeIds)}.");

        // 2. Employee and vacation types
        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<List<VacationBalanceDto>>.NotFoundResponse($"Employee {employeeId} not found.");

        var typeIds = dto.Items.Select(i => i.VacationTypeId).ToList();
        var types   = (await _uow.VacationTypes.GetByIdsNotDeletedAsync(typeIds, ct)).ToDictionary(t => t.Id);
        var missing = typeIds.Where(id => !types.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            return ApiResponse<List<VacationBalanceDto>>.NotFoundResponse(
                $"Vacation type(s) not found: {string.Join(", ", missing)}.");

        // 3. All or nothing: no balance may already exist for (employee, type, year)
        var existing = await _uow.VacationBalances.GetExistingTypeIdsAsync(employeeId, dto.Year, typeIds, ct);
        if (existing.Count > 0)
            return ApiResponse<List<VacationBalanceDto>>.ErrorResponse(
                $"Employee {employeeId} already has a {dto.Year} balance for vacation type(s): {string.Join(", ", existing)}. Nothing was saved.", 409);

        // 4. Balances + ledger rows, saved together
        var now     = DateTimeOffset.UtcNow;
        var created = new List<VacationBalance>();

        foreach (var item in dto.Items)
        {
            var entitled    = item.Entitled ?? types[item.VacationTypeId].DefaultDays;
            var carriedOver = item.CarriedOver ?? 0m;

            var balance = new VacationBalance
            {
                EmployeeId     = employeeId,
                VacationTypeId = item.VacationTypeId,
                Year           = dto.Year,
                Entitled       = entitled,
                CarriedOver    = carriedOver,
                Used           = 0m,
                CreatedAt      = now
            };
            await _uow.VacationBalances.AddAsync(balance);

            if (entitled > 0)
                await _uow.VacationBalances.AddTransactionAsync(new VacationTransaction
                {
                    VacationBalance = balance,
                    Days            = entitled,
                    Reason          = VacationTxReason.YearlyGrant,
                    CreatedAt       = now
                }, ct);

            if (carriedOver > 0)
                await _uow.VacationBalances.AddTransactionAsync(new VacationTransaction
                {
                    VacationBalance = balance,
                    Days            = carriedOver,
                    Reason          = VacationTxReason.Manual,
                    CreatedAt       = now
                }, ct);

            created.Add(balance);
        }

        await _uow.SaveChangesAsync();

        return ApiResponse<List<VacationBalanceDto>>.CreatedResponse(
            created.Select(b => b.ToDto(types[b.VacationTypeId].Name)).ToList());
    }
}