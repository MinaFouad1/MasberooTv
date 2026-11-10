using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

/// <summary>
/// Vacation requests. There is no approval flow: every valid request is Approved automatically.
/// </summary>
public class VacationRequestService : IVacationRequestService
{
    private const int    ReasonMaxLength          = 500;
    private const string ConcurrencyErrorMessage  = "Balance changed, please retry.";

    private readonly IUnitOfWork             _uow;
    private readonly IEmployeeLookup         _employeeLookup;
    private readonly IVacationDaysCalculator _daysCalculator;
    private readonly IAttendanceSaveChanges  _saveChanges;

    public VacationRequestService(IUnitOfWork uow, IEmployeeLookup employeeLookup, IVacationDaysCalculator daysCalculator,
        IAttendanceSaveChanges saveChanges)
    {
        _uow            = uow;
        _employeeLookup = employeeLookup;
        _daysCalculator = daysCalculator;
        _saveChanges    = saveChanges;
    }

    public async Task<ApiResponse<List<VacationRequestDto>>> GetAllAsync(int? employeeId, string? status, CancellationToken ct = default)
    {
        RequestStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<RequestStatus>(status.Trim(), true, out var parsed) || !Enum.IsDefined(parsed))
                return ApiResponse<List<VacationRequestDto>>.ErrorResponse(
                    $"Status must be one of: {string.Join(", ", Enum.GetNames<RequestStatus>())}.");
            statusFilter = parsed;
        }

        var requests = await _uow.VacationRequests.SearchAsync(employeeId, statusFilter, ct);
        return ApiResponse<List<VacationRequestDto>>.SuccessResponse(
            requests.Select(r => r.ToDto(r.VacationType.Name)).ToList());
    }

    public async Task<ApiResponse<VacationRequestDto>> CreateAsync(CreateVacationRequestDto dto, CancellationToken ct = default)
    {
        // Rule 1: employee exists; vacation type exists and is not deleted
        if (!await _employeeLookup.ExistsAsync(dto.EmployeeId, ct))
            return ApiResponse<VacationRequestDto>.NotFoundResponse($"Employee {dto.EmployeeId} not found.");

        var type = await _uow.VacationTypes.GetByIdNotDeletedAsync(dto.VacationTypeId, ct);
        if (type is null)
            return ApiResponse<VacationRequestDto>.NotFoundResponse($"Vacation type {dto.VacationTypeId} not found.");

        // Rule 2: ToDate >= FromDate
        if (dto.FromDate is null || dto.ToDate is null)
            return ApiResponse<VacationRequestDto>.ErrorResponse("FromDate and ToDate are required (yyyy-MM-dd).");

        var fromDate = dto.FromDate.Value;
        var toDate   = dto.ToDate.Value;

        if (toDate < fromDate)
            return ApiResponse<VacationRequestDto>.ErrorResponse("ToDate must be on or after FromDate.");

        // Rule 3: same calendar year
        if (fromDate.Year != toDate.Year)
            return ApiResponse<VacationRequestDto>.ErrorResponse(
                "FromDate and ToDate must be in the same calendar year. Please split the vacation into two requests.");

        var year = fromDate.Year;

        // Rule 4: a balance must exist for (employee, type, year)
        var balance = await _uow.VacationBalances.GetForUpdateAsync(dto.EmployeeId, dto.VacationTypeId, year, ct);
        if (balance is null)
            return ApiResponse<VacationRequestDto>.NotFoundResponse(
                $"Employee {dto.EmployeeId} has no {year} balance for vacation type '{type.Name}'.");

        // Rule 5: days are calculated, never taken from the client
        var days = await _daysCalculator.CalculateDaysAsync(dto.EmployeeId, fromDate, toDate, ct);
        if (days <= 0)
            return ApiResponse<VacationRequestDto>.ErrorResponse("No working days in this period.");

        // Rule 6: no overlap with another approved request
        if (await _uow.VacationRequests.HasApprovedOverlapAsync(dto.EmployeeId, fromDate, toDate, ct))
            return ApiResponse<VacationRequestDto>.ErrorResponse(
                "The employee already has an approved vacation request that overlaps this period.", 409);

        // Rule 7: enough balance
        var remaining = balance.Entitled + balance.CarriedOver - balance.Used;
        if (days > remaining)
            return ApiResponse<VacationRequestDto>.ErrorResponse(
                $"Not enough balance. Remaining: {remaining} day(s), requested: {days} day(s).", 409);

        var reason = dto.Reason?.Trim() ?? string.Empty;
        if (reason.Length > ReasonMaxLength)
            return ApiResponse<VacationRequestDto>.ErrorResponse($"Reason must be at most {ReasonMaxLength} characters.");

        // Write: request (auto-approved) + balance.Used + ledger row, in ONE SaveChanges
        var now = DateTimeOffset.UtcNow;

        var request = new VacationRequest
        {
            EmployeeId     = dto.EmployeeId,
            VacationTypeId = dto.VacationTypeId,
            FromDate       = fromDate,
            ToDate         = toDate,
            Days           = days,
            Status         = RequestStatus.Approved,
            ApprovedBy     = null, // auto-approved, no admin
            Reason         = reason,
            CreatedAt      = now
        };
        await _uow.VacationRequests.AddAsync(request);

        balance.Used     += days;
        balance.UpdatedAt = now;

        await _uow.VacationBalances.AddTransactionAsync(new VacationTransaction
        {
            VacationBalance = balance,
            VacationRequest = request,
            Days            = -days,
            Reason          = VacationTxReason.Request,
            CreatedAt       = now
        }, ct);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<VacationRequestDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<VacationRequestDto>.CreatedResponse(request.ToDto(type.Name));
    }

    public async Task<ApiResponse<VacationRequestDto>> WithdrawAsync(int id, CancellationToken ct = default)
    {
        var request = await _uow.VacationRequests.GetByIdNotDeletedAsync(id, ct);
        if (request is null)
            return ApiResponse<VacationRequestDto>.NotFoundResponse($"Vacation request {id} not found.");

        if (request.Status == RequestStatus.Cancelled)
            return ApiResponse<VacationRequestDto>.ErrorResponse($"Vacation request {id} is already cancelled.", 409);

        if (request.Status != RequestStatus.Approved)
            return ApiResponse<VacationRequestDto>.ErrorResponse("Only approved vacation requests can be withdrawn.", 409);

        var balance = await _uow.VacationBalances.GetForUpdateAsync(
            request.EmployeeId, request.VacationTypeId, request.FromDate.Year, ct);
        if (balance is null)
            return ApiResponse<VacationRequestDto>.NotFoundResponse(
                $"No {request.FromDate.Year} balance found for employee {request.EmployeeId} and vacation type {request.VacationTypeId}.");

        // Write: cancel (no hard delete, no IsDeleted) + give the days back + ledger row, in ONE SaveChanges
        var now = DateTimeOffset.UtcNow;

        request.Status    = RequestStatus.Cancelled;
        request.UpdatedAt = now;

        balance.Used      = Math.Max(0m, balance.Used - request.Days);
        balance.UpdatedAt = now;

        await _uow.VacationBalances.AddTransactionAsync(new VacationTransaction
        {
            VacationBalance = balance,
            VacationRequest = request,
            Days            = request.Days,
            Reason          = VacationTxReason.Request,
            CreatedAt       = now
        }, ct);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<VacationRequestDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<VacationRequestDto>.SuccessResponse(request.ToDto(request.VacationType.Name),
            "Vacation request withdrawn successfully.");
    }
}