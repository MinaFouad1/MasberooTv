using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public class PermissionRequestService : IPermissionRequestService
{
    private const int    MinMinutes              = 1;
    private const int    MaxMinutes              = 480;
    private const int    MinYear                 = 2000;
    private const int    MaxYear                 = 2100;
    private const int    ReasonMaxLength         = 500; // column length
    private const string NoBalanceMessage        = "No permission balance is assigned for this employee, permission type and month. Assign a balance first.";
    private const string ConcurrencyErrorMessage = "Balance changed, please retry.";

    private readonly IUnitOfWork                _uow;
    private readonly IEmployeeLookup            _employeeLookup;
    private readonly IPermissionUsageCalculator _usageCalculator;
    private readonly IConflictAwareSaveChanges  _saveChanges;

    public PermissionRequestService(IUnitOfWork uow, IEmployeeLookup employeeLookup,
        IPermissionUsageCalculator usageCalculator, IConflictAwareSaveChanges saveChanges)
    {
        _uow             = uow;
        _employeeLookup  = employeeLookup;
        _usageCalculator = usageCalculator;
        _saveChanges     = saveChanges;
    }

    public async Task<ApiResponse<List<PermissionRequestDto>>> GetAllAsync(int? employeeId, int? permissionTypeId, RequestStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        if (from is not null && to is not null && from > to)
            return ApiResponse<List<PermissionRequestDto>>.ErrorResponse("from must be on or before to.");

        return ApiResponse<List<PermissionRequestDto>>.SuccessResponse(
            await _uow.PermissionRequests.SearchAsync(employeeId, permissionTypeId, status, from, to, ct));
    }

    public async Task<ApiResponse<PermissionRequestDto>> CreateAsync(CreatePermissionRequestDto dto, CancellationToken ct = default)
    {
        // 1. Employee and permission type
        if (!await _employeeLookup.ExistsAsync(dto.EmployeeId, ct))
            return ApiResponse<PermissionRequestDto>.NotFoundResponse($"Employee {dto.EmployeeId} not found.");

        var type = await _uow.PermissionTypes.GetByIdNotDeletedAsync(dto.PermissionTypeId, ct);
        if (type is null)
            return ApiResponse<PermissionRequestDto>.NotFoundResponse($"Permission type {dto.PermissionTypeId} not found.");
        if (!type.IsActive)
            return ApiResponse<PermissionRequestDto>.ErrorResponse($"Permission type '{type.Name}' is not active.");

        // 2. Minutes and date
        if (dto.Minutes < MinMinutes || dto.Minutes > MaxMinutes)
            return ApiResponse<PermissionRequestDto>.ErrorResponse($"Minutes must be between {MinMinutes} and {MaxMinutes}.");
        if (dto.PermissionDate is null)
            return ApiResponse<PermissionRequestDto>.ErrorResponse("PermissionDate is required.");
        var date = dto.PermissionDate.Value;
        if (date.Year < MinYear || date.Year > MaxYear)
            return ApiResponse<PermissionRequestDto>.ErrorResponse($"PermissionDate year must be between {MinYear} and {MaxYear}.");
        var reason = dto.Reason?.Trim() ?? string.Empty;
        if (reason.Length > ReasonMaxLength)
            return ApiResponse<PermissionRequestDto>.ErrorResponse($"Reason must be at most {ReasonMaxLength} characters.");

        // 3. The month's balance must already exist (never created here)
        var balance = await _uow.PermissionBalances.GetForUpdateAsync(dto.EmployeeId, type.Id, date.Year, date.Month, ct);
        if (balance is null)
            return ApiResponse<PermissionRequestDto>.ErrorResponse(NoBalanceMessage);

        // 4. Auto-approved request
        var now = DateTimeOffset.UtcNow;
        var request = new PermissionRequest
        {
            EmployeeId       = dto.EmployeeId,
            PermissionTypeId = type.Id,
            PermissionDate   = date,
            Minutes          = dto.Minutes,
            ExceededMinutes  = 0,
            Reason           = reason,
            Status           = RequestStatus.Approved,
            ApprovedBy       = null, // auto-approved
            AttendanceId     = null,
            CreatedAt        = now
        };

        // 5. RECOMPUTE the group including the new request (last among equal dates)
        var group   = await _uow.PermissionRequests.GetApprovedGroupForUpdateAsync(dto.EmployeeId, type.Id, date.Year, date.Month, ct);
        var ordered = group.Append(request)
                           .OrderBy(r => r.PermissionDate)
                           .ThenBy(r => ReferenceEquals(r, request) ? 1 : 0)
                           .ThenBy(r => r.Id)
                           .ToList();

        await _uow.PermissionRequests.AddAsync(request);
        PermissionUsageRecalculation.Apply(_usageCalculator, balance, ordered, now);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<PermissionRequestDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<PermissionRequestDto>.CreatedResponse(request.ToDto(type.Name, balance),
            request.ExceededMinutes > 0
                ? "Permission request approved. It exceeds the monthly allowance."
                : "Permission request approved.");
    }

    public async Task<ApiResponse<PermissionRequestDto>> WithdrawAsync(int id, CancellationToken ct = default)
    {
        var request = await _uow.PermissionRequests.GetByIdNotDeletedAsync(id, ct);
        if (request is null)
            return ApiResponse<PermissionRequestDto>.NotFoundResponse($"Permission request {id} not found.");
        if (request.Status != RequestStatus.Approved)
            return ApiResponse<PermissionRequestDto>.ErrorResponse(
                $"Only approved permission requests can be withdrawn (current status: {request.Status}).", 409);

        // Cancel (no IsDeleted), then RECOMPUTE the group without it
        var now  = DateTimeOffset.UtcNow;
        var date = request.PermissionDate;
        request.Status          = RequestStatus.Cancelled;
        request.ExceededMinutes = 0;
        request.UpdatedAt       = now;

        var balance = await _uow.PermissionBalances.GetForUpdateAsync(request.EmployeeId, request.PermissionTypeId, date.Year, date.Month, ct);
        if (balance is not null)
        {
            var group     = await _uow.PermissionRequests.GetApprovedGroupForUpdateAsync(request.EmployeeId, request.PermissionTypeId, date.Year, date.Month, ct);
            var remaining = group.Where(r => r.Id != request.Id).ToList();
            PermissionUsageRecalculation.Apply(_usageCalculator, balance, remaining, now);
        }

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<PermissionRequestDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<PermissionRequestDto>.SuccessResponse(request.ToDto(request.PermissionType.Name, balance),
            "Permission request withdrawn successfully.");
    }
}
