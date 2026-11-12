using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;
using Microsoft.Extensions.Options;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Attendances;

public class DefaultAttendanceCalculator : IAttendanceCalculator
{
    private readonly IShiftTimeConverter     _timeConverter;
    private readonly AttendanceStatusOptions _options;

    public DefaultAttendanceCalculator(IShiftTimeConverter timeConverter, IOptions<AttendanceStatusOptions> options)
    {
        _timeConverter = timeConverter;
        _options       = options.Value;
    }

    public AttendanceCalculationResult Calculate(AttendanceCalculationInput input)
    {
        var shift    = input.Shift;
        var checkIn  = input.CheckIn;
        var checkOut = input.CheckOut;

        // ExpectedStart = (WorkDate + StartTime) in the shift's zone, as UTC; ExpectedEnd = ExpectedStart + DurationMinutes
        DateTimeOffset? expectedStart = null;
        DateTimeOffset? expectedEnd   = null;
        if (shift is not null)
        {
            var startUtc = _timeConverter.ToUtc(input.WorkDate, shift.StartTime, shift.TimeZoneId);
            expectedStart = new DateTimeOffset(startUtc, TimeSpan.Zero);
            expectedEnd   = expectedStart.Value.AddMinutes(shift.DurationMinutes);
        }

        var spanMinutes = checkIn is not null && checkOut is not null
            ? FloorMinutes(checkOut.Value - checkIn.Value)
            : 0;

        var lateMinutes = shift is not null && checkIn is not null
            ? Math.Max(0, FloorMinutes(checkIn.Value - expectedStart!.Value) - shift.GraceMinutes)
            : 0;

        var earlyLeaveMinutes = shift is not null && checkOut is not null
            ? Math.Max(0, FloorMinutes(expectedEnd!.Value - checkOut.Value))
            : 0;

        var workedMinutes = checkOut is null
            ? 0
            : shift is not null ? Math.Max(0, spanMinutes - shift.BreakMinutes) : spanMinutes;

        var status = ResolveStatus(input, expectedEnd, workedMinutes);

        return new AttendanceCalculationResult(expectedStart, expectedEnd, lateMinutes, earlyLeaveMinutes, workedMinutes, status);
    }

    /// <summary>First match wins, in this exact order.</summary>
    private AttendanceStatus ResolveStatus(AttendanceCalculationInput input, DateTimeOffset? expectedEnd, int workedMinutes)
    {
        var hasAnyPunch = input.CheckIn is not null || input.CheckOut is not null;

        // 1. No shift
        if (input.Shift is null)
            return hasAnyPunch ? AttendanceStatus.Overtime : AttendanceStatus.DayOff;

        // 2. Holiday
        if (input.IsHoliday)
            return hasAnyPunch ? AttendanceStatus.Overtime : AttendanceStatus.Holiday;

        // 3. Approved vacation
        if (input.HasApprovedVacation)
            return AttendanceStatus.OnLeave;

        // 4. Shift not finished yet and no check-out
        if (input.CheckOut is null && input.Now < expectedEnd!.Value)
            return AttendanceStatus.Pending;

        // 5. No punch at all
        if (!hasAnyPunch)
            return AttendanceStatus.Absent;

        // 6. Shift already ended without a check-out
        if (input.CheckOut is null)
            return AttendanceStatus.Incomplete;

        // 7. Worked percent of the required minutes (BreakMinutes < DurationMinutes is enforced on shifts)
        var requiredMinutes = input.Shift.DurationMinutes - input.Shift.BreakMinutes;
        var percent = requiredMinutes > 0 ? (double)workedMinutes / requiredMinutes : 0d;

        if (percent >= _options.PresentThreshold)
            return AttendanceStatus.Present;
        if (percent >= _options.HalfDayThreshold)
            return AttendanceStatus.HalfDay;
        return AttendanceStatus.Absent;
    }

    private static int FloorMinutes(TimeSpan span) => (int)Math.Floor(span.TotalMinutes);
}
