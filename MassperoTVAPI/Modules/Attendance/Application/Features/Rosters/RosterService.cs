using System.Globalization;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions.Repositories;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Rosters;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Rosters;

public class RosterService : IRosterService
{
    private const int    MinYear                 = 2000;
    private const int    MaxYear                 = 2100;
    private const int    MaxEmployees            = 500;
    private const int    MaxRowsPerRequest       = 20000;
    private const int    MaxOverlapsReported     = 50;
    private const int    NoteMaxLength           = 500; // column length
    private const string ConcurrencyErrorMessage = "The roster changed, please retry.";

    private readonly IUnitOfWork               _uow;
    private readonly IEmployeeLookup           _employeeLookup;
    private readonly ITeamMembershipRepository _teams;
    private readonly IShiftOverlapChecker      _overlapChecker;
    private readonly IConflictAwareSaveChanges _saveChanges;

    public RosterService(IUnitOfWork uow, IEmployeeLookup employeeLookup, ITeamMembershipRepository teams,
        IShiftOverlapChecker overlapChecker, IConflictAwareSaveChanges saveChanges)
    {
        _uow            = uow;
        _employeeLookup = employeeLookup;
        _teams          = teams;
        _overlapChecker = overlapChecker;
        _saveChanges    = saveChanges;
    }

    // ---------------------------------------------------------------------
    // 2A. Bulk assign
    // ---------------------------------------------------------------------
    public async Task<ApiResponse<RosterAssignmentResultDto>> AssignAsync(BulkRosterAssignmentDto dto, CancellationToken ct = default)
    {
        // 1. Period
        var periodError = ValidatePeriod(dto.Year, dto.Month);
        if (periodError is not null)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(periodError);

        // 2. Who
        var listedIds = dto.EmployeeIds ?? new List<int>();
        if (dto.TeamId is null && listedIds.Count == 0)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse("Provide teamId, employeeIds or both.");

        // 3. Team (current members: Employee.TeamId = teamId)
        var employeeIds = new HashSet<int>(listedIds);
        if (dto.TeamId is int teamId)
        {
            if (!await _teams.TeamExistsAsync(teamId, ct))
                return ApiResponse<RosterAssignmentResultDto>.NotFoundResponse($"Team {teamId} not found.");
            employeeIds.UnionWith(await _teams.GetMemberIdsAsync(teamId, ct));
        }

        // 4. Resolved set
        if (employeeIds.Count == 0)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse("No employees to assign: the team has no members and no employeeIds were given.");
        if (employeeIds.Count > MaxEmployees)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(
                $"At most {MaxEmployees} employees can be assigned in one request (resolved {employeeIds.Count}).");

        // 5. Every employee exists (one query)
        var existingEmployeeIds = await _employeeLookup.GetExistingIdsAsync(employeeIds, ct);
        var missingEmployees    = employeeIds.Except(existingEmployeeIds).OrderBy(id => id).ToList();
        if (missingEmployees.Count > 0)
            return ApiResponse<RosterAssignmentResultDto>.NotFoundResponse($"Employee(s) not found: {string.Join(", ", missingEmployees)}.");

        // 6. Days (explicit list or weekly pattern)
        if (!TryParseConflictMode(dto.ConflictMode, out var replace))
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse("ConflictMode must be 'Skip' or 'Replace'.");
        if ((dto.Days is null) == (dto.WeeklyPattern is null))
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse("Provide exactly one of days or weeklyPattern.");

        var monthStart = new DateOnly(dto.Year, dto.Month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);
        List<RosterDayInputDto> days;
        if (dto.WeeklyPattern is not null)
        {
            var patternError = ExpandWeeklyPattern(dto.WeeklyPattern, monthStart, monthEnd, out days);
            if (patternError is not null)
                return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(patternError);
        }
        else
        {
            days = dto.Days!;
        }

        var dayError = ValidateDays(days, monthStart, monthEnd);
        if (dayError is not null)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(dayError);

        // 7. Shifts exist, not deleted, active (one query)
        var (shifts, shiftError) = await LoadShiftsAsync(days, ct);
        if (shiftError is not null)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(shiftError);

        // 8. Row guard (upper bound before any row is loaded)
        var rowsPerEmployee = days.Sum(d => RowKeys(d).Count);
        if ((long)rowsPerEmployee * employeeIds.Count > MaxRowsPerRequest)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(
                $"At most {MaxRowsPerRequest} rows can be created in one request ({employeeIds.Count} employees x {rowsPerEmployee} rows).");

        // 9. Header for (teamId or company-wide, year, month): find now; create it (Draft) only after every check passed
        var now    = DateTimeOffset.UtcNow;
        var header = await _uow.RosterMonths.GetForUpdateAsync(dto.TeamId, dto.Year, dto.Month, ct);
        if (header is { Status: RosterStatus.Locked })
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(LockedMessage(new[] { header.Id }), 409);

        // 10. Existing rows of all employees for the window (one query; +/- 1 day for night shifts)
        var firstDate  = days.Min(d => d.Date!.Value);
        var lastDate   = days.Max(d => d.Date!.Value);
        var windowRows = await _uow.ShiftAssignments.GetForEmployeesInRangeForUpdateAsync(
            employeeIds.ToList(), firstDate.AddDays(-1), lastDate.AddDays(1), ct);
        var rowsByEmployee    = windowRows.ToLookup(r => r.EmployeeId);
        var rowsByEmployeeDay = windowRows.ToLookup(r => (r.EmployeeId, r.WorkDate));

        // 11. Plan in memory (nothing is changed yet)
        var plans        = new List<DayPlan>();
        var overlapInput = new List<ScheduledShift>();
        var skipped      = 0;
        var replaced     = 0;
        foreach (var employeeId in employeeIds.OrderBy(id => id))
        {
            var writtenDates = new HashSet<DateOnly>();
            foreach (var day in days)
            {
                var date         = day.Date!.Value;
                var existingRows = rowsByEmployeeDay[(employeeId, date)].ToList();
                if (existingRows.Count > 0 && !replace)
                {
                    skipped++;
                    continue;
                }
                if (existingRows.Count > 0)
                    replaced++;

                var plan = PlanDay(employeeId, date, day, existingRows);
                plans.Add(plan);
                writtenDates.Add(date);
                AddDesiredRows(overlapInput, plan, shifts);
            }

            foreach (var row in rowsByEmployee[employeeId].Where(r => !writtenDates.Contains(r.WorkDate)))
                AddRemainingRow(overlapInput, row);
        }

        // 12. Locked headers of existing rows this request touches
        var lockedIds = plans.SelectMany(p => p.Kept.Concat(p.Removed))
                             .Select(r => r.RosterMonth)
                             .Where(h => h.Status == RosterStatus.Locked)
                             .Select(h => h.Id).Distinct().OrderBy(id => id).ToList();
        if (lockedIds.Count > 0)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(LockedMessage(lockedIds), 409);

        // 13. Stored time zones must resolve (no fabricated instants), then overlaps: all or nothing
        var zoneError = UnresolvableTimeZoneMessage(overlapInput);
        if (zoneError is not null)
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(zoneError);

        var overlaps = _overlapChecker.FindOverlaps(overlapInput, MaxOverlapsReported);
        if (overlaps.Count > 0)
            return ApiConflictResponse<RosterAssignmentResultDto, RosterOverlapDto>.Create(OverlapMessage(overlaps.Count), overlaps);

        // 14. Stage only now: create the header if missing, apply, AddRange, ONE save
        if (header is null)
        {
            header = NewHeader(dto.TeamId, dto.Year, dto.Month, now);
            await _uow.RosterMonths.AddAsync(header);
        }

        var inserted = new List<ShiftAssignment>();
        foreach (var plan in plans)
            ApplyPlan(plan, header, now, inserted);
        await _uow.ShiftAssignments.AddRangeAsync(inserted, ct);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<RosterAssignmentResultDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<RosterAssignmentResultDto>.SuccessResponse(
            new RosterAssignmentResultDto(header.Id, employeeIds.Count, inserted.Count, skipped, replaced),
            "Roster assignments saved successfully.");
    }

    // ---------------------------------------------------------------------
    // 2B. Edit one employee day (replaces the whole day)
    // ---------------------------------------------------------------------
    public async Task<ApiResponse<List<RosterRowDto>>> EditDayAsync(int employeeId, DateOnly date, EditRosterDayDto dto, CancellationToken ct = default)
    {
        var periodError = ValidatePeriod(date.Year, date.Month);
        if (periodError is not null)
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(periodError);

        if (!await _employeeLookup.ExistsAsync(employeeId, ct))
            return ApiResponse<List<RosterRowDto>>.NotFoundResponse($"Employee {employeeId} not found.");

        var monthStart = new DateOnly(date.Year, date.Month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);
        var day        = new RosterDayInputDto { Date = date, DayType = dto.DayType, ShiftIds = dto.ShiftIds, Note = dto.Note };

        var dayError = ValidateDays(new List<RosterDayInputDto> { day }, monthStart, monthEnd);
        if (dayError is not null)
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(dayError);

        var (shifts, shiftError) = await LoadShiftsAsync(new List<RosterDayInputDto> { day }, ct);
        if (shiftError is not null)
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(shiftError);

        // Existing rows of the employee for date - 1 .. date + 1 (one query)
        var now        = DateTimeOffset.UtcNow;
        var windowRows = await _uow.ShiftAssignments.GetForEmployeesInRangeForUpdateAsync(
            new List<int> { employeeId }, date.AddDays(-1), date.AddDays(1), ct);
        var dayRows = windowRows.Where(r => r.WorkDate == date).ToList();

        // Header: the day's own header, else the employee's most used header in the month, else company-wide
        var header = PickMostUsedHeader(dayRows);
        if (header is null)
        {
            var monthHeaderIds = await _uow.ShiftAssignments.GetRosterMonthIdsAsync(employeeId, monthStart, monthEnd, ct);
            var pickedId = monthHeaderIds.GroupBy(id => id)
                                         .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                                         .Select(g => (int?)g.Key)
                                         .FirstOrDefault();
            if (pickedId is int id)
                header = await _uow.RosterMonths.GetByIdNotDeletedAsync(id, ct);
        }
        if (header is null)
            header = await _uow.RosterMonths.GetForUpdateAsync(null, date.Year, date.Month, ct); // may stay null: created after the checks

        var plan = PlanDay(employeeId, date, day, dayRows);

        // Locked: the target header and every header of the rows being replaced
        var lockedIds = plan.Kept.Concat(plan.Removed).Select(r => r.RosterMonth)
                            .Concat(header is null ? Array.Empty<RosterMonth>() : new[] { header })
                            .Where(h => h.Status == RosterStatus.Locked)
                            .Select(h => h.Id).Distinct().OrderBy(id => id).ToList();
        if (lockedIds.Count > 0)
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(LockedMessage(lockedIds), 409);

        // Overlaps: new rows + neighbouring rows that remain (the replaced rows are excluded)
        var overlapInput = new List<ScheduledShift>();
        AddDesiredRows(overlapInput, plan, shifts);
        foreach (var row in windowRows.Where(r => r.WorkDate != date))
            AddRemainingRow(overlapInput, row);

        var zoneError = UnresolvableTimeZoneMessage(overlapInput);
        if (zoneError is not null)
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(zoneError);

        var overlaps = _overlapChecker.FindOverlaps(overlapInput, MaxOverlapsReported);
        if (overlaps.Count > 0)
            return ApiConflictResponse<List<RosterRowDto>, RosterOverlapDto>.Create(OverlapMessage(overlaps.Count), overlaps);

        // Stage only now: create the company-wide header if missing, apply, AddRange, ONE save
        if (header is null)
        {
            header = NewHeader(null, date.Year, date.Month, now);
            await _uow.RosterMonths.AddAsync(header);
        }

        var inserted = new List<ShiftAssignment>();
        ApplyPlan(plan, header, now, inserted);
        await _uow.ShiftAssignments.AddRangeAsync(inserted, ct);

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<List<RosterRowDto>>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        var result = plan.Kept.Concat(inserted)
                         .Select(r => ToRowDto(r, shifts))
                         .OrderBy(r => r.StartTime).ThenBy(r => r.Id)
                         .ToList();
        return ApiResponse<List<RosterRowDto>>.SuccessResponse(result, "Roster day saved successfully.");
    }

    // ---------------------------------------------------------------------
    // 2C. View schedule (read only)
    // ---------------------------------------------------------------------
    public async Task<ApiResponse<List<RosterEmployeeScheduleDto>>> GetScheduleAsync(int year, int month, int? teamId, int? employeeId, CancellationToken ct = default)
    {
        var periodError = ValidatePeriod(year, month);
        if (periodError is not null)
            return ApiResponse<List<RosterEmployeeScheduleDto>>.ErrorResponse(periodError);

        if (teamId is int team && !await _teams.TeamExistsAsync(team, ct))
            return ApiResponse<List<RosterEmployeeScheduleDto>>.NotFoundResponse($"Team {team} not found.");
        if (employeeId is int employee && !await _employeeLookup.ExistsAsync(employee, ct))
            return ApiResponse<List<RosterEmployeeScheduleDto>>.NotFoundResponse($"Employee {employee} not found.");

        var monthStart = new DateOnly(year, month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);
        var rows       = await _uow.ShiftAssignments.GetScheduleAsync(monthStart, monthEnd, teamId, employeeId, ct);

        // Rows arrive ordered by employee name, date, shift start time; GroupBy keeps that order.
        var result = rows.GroupBy(r => r.EmployeeId)
                         .Select(e => new RosterEmployeeScheduleDto(
                             e.Key,
                             e.First().EmployeeName,
                             e.GroupBy(r => r.Date)
                              .Select(d => new RosterDayScheduleDto(
                                  d.Key,
                                  d.Select(r => new RosterRowDto(r.Id, r.Date, r.DayType, r.ShiftId, r.ShiftName, r.StartTime, r.DurationMinutes, r.Note))
                                   .ToList()))
                              .ToList()))
                         .ToList();

        return ApiResponse<List<RosterEmployeeScheduleDto>>.SuccessResponse(result);
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    /// <summary>What happens to one employee-day. Matching existing rows are REUSED (same Id and creation data).</summary>
    private sealed class DayPlan
    {
        public required int      EmployeeId { get; init; }
        public required DateOnly Date       { get; init; }
        public required DayType  DayType    { get; init; }
        public string?           Note       { get; init; }
        public List<ShiftAssignment> Kept        { get; } = new(); // existing rows reused for the new day
        public List<ShiftAssignment> Removed     { get; } = new(); // existing rows no longer needed -> soft delete
        public List<int?>            NewShiftIds { get; } = new(); // rows to insert (null = day off)
    }

    private static DayPlan PlanDay(int employeeId, DateOnly date, RosterDayInputDto day, List<ShiftAssignment> existingDayRows)
    {
        var plan = new DayPlan
        {
            EmployeeId = employeeId,
            Date       = date,
            DayType    = day.DayType!.Value,
            Note       = string.IsNullOrWhiteSpace(day.Note) ? null : day.Note.Trim()
        };

        var unmatched = new List<ShiftAssignment>(existingDayRows);
        foreach (var key in RowKeys(day))
        {
            // Unique index (EmployeeId, WorkDate, ShiftId): at most one existing row per key.
            var match = unmatched.FirstOrDefault(r => r.ShiftId == key);
            if (match is not null)
            {
                unmatched.Remove(match);
                plan.Kept.Add(match);
            }
            else
            {
                plan.NewShiftIds.Add(key);
            }
        }

        plan.Removed.AddRange(unmatched);
        return plan;
    }

    private static void ApplyPlan(DayPlan plan, RosterMonth header, DateTimeOffset now, List<ShiftAssignment> inserted)
    {
        foreach (var row in plan.Kept)
        {
            row.DayType     = plan.DayType;
            row.Note        = plan.Note;
            row.RosterMonth = header;
            row.UpdatedAt   = now;
        }

        foreach (var row in plan.Removed)
        {
            row.IsDeleted = true;
            row.UpdatedAt = now;
        }

        foreach (var shiftId in plan.NewShiftIds)
        {
            inserted.Add(new ShiftAssignment
            {
                RosterMonth = header,
                EmployeeId  = plan.EmployeeId,
                WorkDate    = plan.Date,
                DayType     = plan.DayType,
                ShiftId     = shiftId,
                Note        = plan.Note,
                CreatedAt   = now
            });
        }
    }

    /// <summary>Work: one key per shift id. DayOff: a single null key (ShiftId = null).</summary>
    private static List<int?> RowKeys(RosterDayInputDto day)
        => day.DayType == DayType.Work
            ? (day.ShiftIds ?? new List<int>()).Select(id => (int?)id).ToList()
            : new List<int?> { null };

    private static void AddDesiredRows(List<ScheduledShift> input, DayPlan plan, Dictionary<int, Shift> shifts)
    {
        foreach (var shiftId in plan.Kept.Select(r => r.ShiftId).Concat(plan.NewShiftIds))
        {
            if (shiftId is int id && shifts.TryGetValue(id, out var shift))
                input.Add(new ScheduledShift(plan.EmployeeId, plan.Date, id, shift.StartTime, shift.DurationMinutes, shift.TimeZoneId, true));
        }
    }

    private static void AddRemainingRow(List<ScheduledShift> input, ShiftAssignment row)
    {
        if (row.ShiftId is int id && row.Shift is not null)
            input.Add(new ScheduledShift(row.EmployeeId, row.WorkDate, id, row.Shift.StartTime, row.Shift.DurationMinutes, row.Shift.TimeZoneId, false));
    }

    private static RosterMonth? PickMostUsedHeader(List<ShiftAssignment> rows)
        => rows.Where(r => !r.RosterMonth.IsDeleted)
               .GroupBy(r => r.RosterMonthId)
               .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
               .Select(g => g.First().RosterMonth)
               .FirstOrDefault();

    private static RosterMonth NewHeader(int? teamId, int year, int month, DateTimeOffset now) => new()
    {
        TeamId    = teamId,
        Year      = year,
        Month     = month,
        Status    = RosterStatus.Draft,
        CreatedAt = now
    };

    private async Task<(Dictionary<int, Shift> Shifts, string? Error)> LoadShiftsAsync(List<RosterDayInputDto> days, CancellationToken ct)
    {
        var requested = days.SelectMany(d => d.ShiftIds ?? new List<int>()).Distinct().ToList();
        var shifts    = (await _uow.Shifts.GetByIdsNotDeletedAsync(requested, ct)).ToDictionary(s => s.Id);
        var bad       = requested.Where(id => !shifts.TryGetValue(id, out var s) || !s.IsActive).OrderBy(id => id).ToList();
        return bad.Count > 0
            ? (shifts, $"Shift(s) not found, deleted or inactive: {string.Join(", ", bad)}.")
            : (shifts, null);
    }

    private static string? ValidatePeriod(int year, int month)
    {
        if (month < 1 || month > 12)
            return "Month must be between 1 and 12.";
        if (year < MinYear || year > MaxYear)
            return $"Year must be between {MinYear} and {MaxYear}.";
        return null;
    }

    private static bool TryParseConflictMode(string? value, out bool replace)
    {
        replace = false;
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("Skip", StringComparison.OrdinalIgnoreCase))
            return true;
        if (value.Trim().Equals("Replace", StringComparison.OrdinalIgnoreCase))
        {
            replace = true;
            return true;
        }
        return false;
    }

    private static string? ExpandWeeklyPattern(RosterWeeklyPatternDto pattern, DateOnly monthStart, DateOnly monthEnd, out List<RosterDayInputDto> days)
    {
        days = new List<RosterDayInputDto>();
        var from = pattern.DateFrom ?? monthStart;
        var to   = pattern.DateTo ?? monthEnd;
        if (from < monthStart || from > monthEnd || to < monthStart || to > monthEnd)
            return $"weeklyPattern dateFrom and dateTo must be inside {Format(monthStart, "yyyy-MM")}.";
        if (from > to)
            return "weeklyPattern dateFrom must be on or before dateTo.";

        var items = pattern.Items ?? new List<RosterWeeklyItemDto>();
        if (items.Any(i => i.DayOfWeek is null))
            return "Every weeklyPattern item needs a dayOfWeek.";
        var duplicates = items.GroupBy(i => i.DayOfWeek!.Value).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            return $"Duplicate dayOfWeek in weeklyPattern: {string.Join(", ", duplicates)}.";

        // Dates without a matching item are NOT assigned (they are not turned into days off).
        var byDayOfWeek = items.ToDictionary(i => i.DayOfWeek!.Value);
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (byDayOfWeek.TryGetValue(date.DayOfWeek, out var item))
                days.Add(new RosterDayInputDto { Date = date, DayType = item.DayType, ShiftIds = item.ShiftIds, Note = item.Note });
        }
        return null;
    }

    private static string? ValidateDays(List<RosterDayInputDto> days, DateOnly monthStart, DateOnly monthEnd)
    {
        if (days.Count == 0)
            return "At least one day is required.";
        if (days.Any(d => d.Date is null))
            return "Every day needs a date.";

        var outside = days.Select(d => d.Date!.Value).Where(d => d < monthStart || d > monthEnd).Distinct().OrderBy(d => d).ToList();
        if (outside.Count > 0)
            return $"Date(s) outside {Format(monthStart, "yyyy-MM")}: {string.Join(", ", outside.Select(d => Format(d, "yyyy-MM-dd")))}.";

        var duplicates = days.GroupBy(d => d.Date!.Value).Where(g => g.Count() > 1).Select(g => g.Key).OrderBy(d => d).ToList();
        if (duplicates.Count > 0)
            return $"Duplicate date(s): {string.Join(", ", duplicates.Select(d => Format(d, "yyyy-MM-dd")))}.";

        foreach (var day in days.OrderBy(d => d.Date))
        {
            var label    = Format(day.Date!.Value, "yyyy-MM-dd");
            var shiftIds = day.ShiftIds ?? new List<int>();
            if (day.DayType is null)
                return $"dayType is required ({label}).";
            if (day.DayType == DayType.Work && shiftIds.Count == 0)
                return $"A Work day needs at least one shiftId ({label}).";
            if (day.DayType == DayType.Work && shiftIds.Distinct().Count() != shiftIds.Count)
                return $"shiftIds must be distinct ({label}).";
            if (day.DayType == DayType.DayOff && shiftIds.Count > 0)
                return $"A DayOff day must not have shiftIds ({label}).";
            if (day.Note is not null && day.Note.Trim().Length > NoteMaxLength)
                return $"Note must be at most {NoteMaxLength} characters ({label}).";
        }
        return null;
    }

    private static RosterRowDto ToRowDto(ShiftAssignment row, Dictionary<int, Shift> shifts)
    {
        Shift? shift = row.ShiftId is int id && shifts.TryGetValue(id, out var s) ? s : null;
        return new RosterRowDto(row.Id, row.WorkDate, row.DayType, row.ShiftId, shift?.Name, shift?.StartTime, shift?.DurationMinutes, row.Note);
    }

    private string? UnresolvableTimeZoneMessage(List<ScheduledShift> rows)
    {
        var zones = _overlapChecker.FindUnresolvableTimeZones(rows);
        if (zones.Count == 0)
            return null;

        var shiftIds = rows.Where(r => zones.Contains(r.TimeZoneId)).Select(r => r.ShiftId).Distinct().OrderBy(id => id);
        return $"Shift(s) {string.Join(", ", shiftIds)} have a TimeZoneId that cannot be resolved on this server ('{string.Join("', '", zones)}'). Fix the shift first. Nothing was saved.";
    }

    private static string LockedMessage(IEnumerable<int> rosterMonthIds)
        => $"Roster month(s) {string.Join(", ", rosterMonthIds)} are locked and cannot be changed.";

    private static string OverlapMessage(int count)
        => count >= MaxOverlapsReported
            ? $"At least {count} overlapping shift(s) found (first {MaxOverlapsReported} listed). Nothing was saved."
            : $"{count} overlapping shift(s) found. Nothing was saved.";

    private static string Format(DateOnly date, string format) => date.ToString(format, CultureInfo.InvariantCulture);
}
