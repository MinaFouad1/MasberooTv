using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Attendances;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

public class AttendanceService : IAttendanceService
{
    private const int    MinYear                 = 2000;
    private const int    MaxYear                 = 2100;
    private const int    ReasonMaxLength         = 500; // column length
    private const int    MaxSpanHours            = 24;
    private const int    EarlyCheckInWindowHours = 12;
    private const int    MaxPageSize             = 100;
    private const string DuplicateMessage        = "An attendance record already exists for this employee, work date and shift.";
    private const string RetryMessage            = "The same attendance record was saved by another request at the same time, please retry.";

    private readonly IUnitOfWork               _uow;
    private readonly IEmployeeLookup           _employeeLookup;
    private readonly IAttendanceCalculator     _calculator;
    private readonly IShiftTimeConverter       _timeConverter;
    private readonly IConflictAwareSaveChanges _saveChanges;

    public AttendanceService(IUnitOfWork uow, IEmployeeLookup employeeLookup, IAttendanceCalculator calculator,
        IShiftTimeConverter timeConverter, IConflictAwareSaveChanges saveChanges)
    {
        _uow            = uow;
        _employeeLookup = employeeLookup;
        _calculator     = calculator;
        _timeConverter  = timeConverter;
        _saveChanges    = saveChanges;
    }

    // -- POST manual ----------------------------------------------------------

    public async Task<ApiResponse<AttendanceDto>> CreateManualAsync(CreateManualAttendanceDto dto, CancellationToken ct = default)
    {
        // 1. Employee exists
        if (!await _employeeLookup.ExistsAsync(dto.EmployeeId, ct))
            return ApiResponse<AttendanceDto>.NotFoundResponse($"Employee {dto.EmployeeId} not found.");

        // 2. Times, work date and reason
        var now   = DateTimeOffset.UtcNow;
        var error = Validate(dto, now, out var workDate, out var checkIn, out var checkOut, out var reason);
        if (error is not null)
            return ApiResponse<AttendanceDto>.ErrorResponse(error);

        // 3. Approved vacation covering the work date
        if (await _uow.VacationRequests.HasApprovedOverlapAsync(dto.EmployeeId, workDate, workDate, ct))
            return ApiResponse<AttendanceDto>.ErrorResponse(
                $"The employee has an approved vacation that covers {workDate:yyyy-MM-dd}. Withdraw the vacation first.", 409);

        // 4. Resolve the shift from the employee's Work assignments on the work date
        var assignedShiftIds = await _uow.ShiftAssignments.GetWorkShiftIdsAsync(dto.EmployeeId, workDate, ct);
        int? shiftId = null;
        if (dto.ShiftId is not null)
        {
            if (!assignedShiftIds.Contains(dto.ShiftId.Value))
                return ApiResponse<AttendanceDto>.ErrorResponse(
                    $"Shift {dto.ShiftId.Value} is not assigned to the employee on {workDate:yyyy-MM-dd} (this shift is not assigned to the employee on this date).");
            shiftId = dto.ShiftId.Value;
        }
        else if (assignedShiftIds.Count == 1)
        {
            shiftId = assignedShiftIds[0];
        }
        else if (assignedShiftIds.Count > 1)
        {
            var assigned = await _uow.Shifts.GetByIdsNotDeletedAsync(assignedShiftIds, ct);
            var listed   = assignedShiftIds.Select(id => assigned.FirstOrDefault(s => s.Id == id) is { } s ? $"{s.Id} ({s.Name})" : id.ToString());
            return ApiResponse<AttendanceDto>.ErrorResponse(
                $"The employee has {assignedShiftIds.Count} shifts assigned on {workDate:yyyy-MM-dd}: {string.Join(", ", listed)}. Send shiftId to choose one.");
        }
        // else: no Work row (no assignment or only DayOff) -> unscheduled, shiftId stays null

        Shift? shift = null;
        if (shiftId is not null)
        {
            shift = await _uow.Shifts.GetByIdNotDeletedAsync(shiftId.Value, ct);
            if (shift is null)
                return ApiResponse<AttendanceDto>.NotFoundResponse($"Shift {shiftId.Value} not found.");
            if (!_timeConverter.CanResolve(shift.TimeZoneId))
                return ApiResponse<AttendanceDto>.ErrorResponse(
                    $"Shift {shift.Id} has a TimeZoneId that cannot be resolved on this server ('{shift.TimeZoneId}'). Fix the shift first. Nothing was saved.");
        }

        // 5. Time window
        if (shift is not null)
        {
            var expectedStart = new DateTimeOffset(_timeConverter.ToUtc(workDate, shift.StartTime, shift.TimeZoneId), TimeSpan.Zero);
            var expectedEnd   = expectedStart.AddMinutes(shift.DurationMinutes);
            var earliest      = expectedStart.AddHours(-EarlyCheckInWindowHours);
            if (checkIn < earliest || checkIn >= expectedEnd)
                return ApiResponse<AttendanceDto>.ErrorResponse(
                    $"CheckIn must be at or after {earliest:yyyy-MM-ddTHH:mm:ssZ} and before {expectedEnd:yyyy-MM-ddTHH:mm:ssZ} (UTC) for shift '{shift.Name}' on {workDate:yyyy-MM-dd}.");
        }
        else
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(AttendanceDefaults.TimeZoneId, out var zone))
                return ApiResponse<AttendanceDto>.ErrorResponse(
                    $"The default time zone '{AttendanceDefaults.TimeZoneId}' cannot be resolved on this server. Nothing was saved.");
            var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(checkIn, zone).DateTime);
            if (localDate != workDate)
                return ApiResponse<AttendanceDto>.ErrorResponse(
                    $"Without an assigned shift, CheckIn must fall on {workDate:yyyy-MM-dd} in '{AttendanceDefaults.TimeZoneId}' (it falls on {localDate:yyyy-MM-dd}).");
        }

        // 6. Duplicate (a null shift counts as its own value)
        if (await _uow.Attendances.ExistsForEmployeeDateShiftAsync(dto.EmployeeId, workDate, shiftId, ct))
            return ApiResponse<AttendanceDto>.ErrorResponse(DuplicateMessage, 409);

        // 7. Holiday + calculation (an approved vacation was already rejected in step 3)
        var isHoliday = await _uow.Holidays.DateExistsAsync(workDate, null, ct);
        var result = _calculator.Calculate(new AttendanceCalculationInput(
            shift is null ? null : new AttendanceShiftInfo(shift.StartTime, shift.DurationMinutes, shift.BreakMinutes, shift.GraceMinutes, shift.TimeZoneId),
            workDate, checkIn, checkOut, isHoliday, HasApprovedVacation: false, now));

        // 8. One SaveChanges: the attendance row and its Manual punches
        var attendance = new Attendance
        {
            EmployeeId        = dto.EmployeeId,
            WorkDate          = workDate,
            CheckIn           = checkIn,
            CheckOut          = checkOut,
            ShiftId           = shiftId,
            ExpectedStart     = result.ExpectedStart,
            ExpectedEnd       = result.ExpectedEnd,
            LateMinutes       = result.LateMinutes,
            EarlyLeaveMinutes = result.EarlyLeaveMinutes,
            WorkedMinutes     = result.WorkedMinutes,
            Status            = result.Status,
            IsManualOverride  = true,
            OverrideReason    = reason,
            IsLocked          = false,
            CreatedAt         = now
        };
        await _uow.Attendances.AddAsync(attendance);
        await _uow.AttendancePunches.AddAsync(NewManualPunch(dto.EmployeeId, checkIn, now));
        if (checkOut is not null)
            await _uow.AttendancePunches.AddAsync(NewManualPunch(dto.EmployeeId, checkOut.Value, now));

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<AttendanceDto>.ErrorResponse(RetryMessage, 409);
        }

        // Response: same shape as one search item, permissions of that day filled
        var created = await _uow.Attendances.GetDtoByIdAsync(attendance.Id, ct);
        if (created is null)
            return ApiResponse<AttendanceDto>.NotFoundResponse($"Attendance {attendance.Id} not found.");
        var permissions = await _uow.PermissionRequests.GetApprovedForEmployeesInRangeAsync(new[] { dto.EmployeeId }, workDate, workDate, ct);
        created.Permissions = permissions.Select(ToPermissionDto).ToList();

        return ApiResponse<AttendanceDto>.CreatedResponse(created, "Manual attendance created.");
    }

    // -- GET search -----------------------------------------------------------

    public async Task<ApiResponse<AttendancePagedResultDto>> SearchAsync(AttendanceSearchQueryDto query, CancellationToken ct = default)
    {
        if (query.StartDate is not null && query.EndDate is not null && query.StartDate > query.EndDate)
            return ApiResponse<AttendancePagedResultDto>.ErrorResponse("startDate must be on or before endDate.");

        AttendanceStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!TryParseStatus(query.Status, out var parsed))
                return ApiResponse<AttendancePagedResultDto>.ErrorResponse(
                    $"Invalid status '{query.Status}'. Allowed values: {string.Join(", ", Enum.GetNames<AttendanceStatus>())}.");
            status = parsed;
        }

        if (query.PageNumber < 1)
            return ApiResponse<AttendancePagedResultDto>.ErrorResponse("pageNumber must be at least 1.");
        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            return ApiResponse<AttendancePagedResultDto>.ErrorResponse($"pageSize must be between 1 and {MaxPageSize}.");

        var employeeName = string.IsNullOrWhiteSpace(query.EmployeeName) ? null : query.EmployeeName.Trim();
        var filter = new AttendanceSearchFilter(query.StartDate, query.EndDate, query.ShiftId, query.EmployeeId,
            employeeName, query.JobId, query.PermissionTypeId, status);

        // ONE aggregate query over ALL matching rows (also gives totalCount)
        var stats = await _uow.Attendances.GetStatisticsAsync(filter, ct);

        var items = stats.TotalRecords == 0
            ? new List<AttendanceDto>()
            : await _uow.Attendances.SearchPageAsync(filter, query.PageNumber, query.PageSize, ct);

        // Permissions of the page: ONE query (page employees, page date range), grouped in memory
        if (items.Count > 0)
        {
            var employeeIds = items.Select(i => i.EmployeeId).Distinct().ToList();
            var fromDate    = items.Min(i => i.WorkDate);
            var toDate      = items.Max(i => i.WorkDate);
            var permissions = await _uow.PermissionRequests.GetApprovedForEmployeesInRangeAsync(employeeIds, fromDate, toDate, ct);
            var byDay       = permissions.ToLookup(p => (p.EmployeeId, p.PermissionDate));
            foreach (var item in items)
                item.Permissions = byDay[(item.EmployeeId, item.WorkDate)].Select(ToPermissionDto).ToList();
        }

        var totalPages = (int)Math.Ceiling(stats.TotalRecords / (double)query.PageSize);
        var avgLate    = stats.AvgLateMinutes is null ? 0d : Math.Round(stats.AvgLateMinutes.Value, 1, MidpointRounding.AwayFromZero);
        var statistics = new AttendanceStatisticsDto(
            TotalRecords:       stats.TotalRecords,
            AvgLateMinutes:     avgLate,
            TotalBreakOverages: 0, // placeholder: actual break times are not stored, so this cannot be calculated
            TotalAbsences:      stats.TotalAbsences);

        return ApiResponse<AttendancePagedResultDto>.SuccessResponse(new AttendancePagedResultDto(
            items, query.PageNumber, query.PageSize, stats.TotalRecords, totalPages, statistics));
    }

    // -- Helpers --------------------------------------------------------------

    private static string? Validate(CreateManualAttendanceDto dto, DateTimeOffset now, out DateOnly workDate,
        out DateTimeOffset checkIn, out DateTimeOffset? checkOut, out string? reason)
    {
        workDate = default;
        checkIn  = default;
        checkOut = null;
        reason   = null;

        if (dto.CheckIn is null)
            return "CheckIn is required.";
        checkIn  = dto.CheckIn.Value.ToUniversalTime();
        checkOut = dto.CheckOut?.ToUniversalTime();

        if (checkOut is not null)
        {
            if (checkOut.Value <= checkIn)
                return "CheckOut must be after CheckIn.";
            if (checkOut.Value - checkIn > TimeSpan.FromHours(MaxSpanHours))
                return $"The time between CheckIn and CheckOut must be at most {MaxSpanHours} hours.";
        }

        if (checkIn > now)
            return "CheckIn cannot be in the future.";
        if (checkOut is not null && checkOut.Value > now)
            return "CheckOut cannot be in the future.";

        if (dto.WorkDate is null)
            return "WorkDate is required.";
        workDate = dto.WorkDate.Value;
        if (workDate.Year < MinYear || workDate.Year > MaxYear)
            return $"WorkDate year must be between {MinYear} and {MaxYear}.";

        var trimmed = dto.Reason?.Trim();
        reason = string.IsNullOrEmpty(trimmed) ? null : trimmed;
        if (reason is not null && reason.Length > ReasonMaxLength)
            return $"Reason must be at most {ReasonMaxLength} characters.";

        return null;
    }

    /// <summary>Case-insensitive, one enum NAME only (numbers such as "3" and lists such as "Present,Absent" are rejected).</summary>
    private static bool TryParseStatus(string value, out AttendanceStatus status)
    {
        status = default;
        var text = value.Trim();
        if (text.Length == 0 || text.Contains(',') || int.TryParse(text, out _))
            return false;
        return Enum.TryParse(text, ignoreCase: true, out status) && Enum.IsDefined(status);
    }

    private static AttendancePunch NewManualPunch(int employeeId, DateTimeOffset punchTime, DateTimeOffset now) => new()
    {
        EmployeeId  = employeeId,
        PunchTime   = punchTime,
        DeviceId    = null,
        Source      = PunchSource.Manual,
        IsProcessed = true,
        CreatedAt   = now
    };

    private static AttendancePermissionDto ToPermissionDto(AttendanceDayPermissionDto p)
        => new(p.PermissionRequestId, p.PermissionTypeId, p.PermissionTypeName, p.Minutes, p.ExceededMinutes);
}
