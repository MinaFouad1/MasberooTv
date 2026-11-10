using System.Globalization;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Shifts;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Shifts;

public class ShiftService : IShiftService
{
    private const string DefaultTimeZoneId = "Egypt Standard Time";
    private const int    NameMaxLength     = 100;
    private const int    MaxDuration       = 1440;
    private const int    MaxGrace          = 240;
    private static readonly string[] StartTimeFormats = ["HH:mm", "HH:mm:ss"];

    private readonly IUnitOfWork _uow;

    public ShiftService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<List<ShiftDto>>> GetAllAsync(string? name, bool? isActive, CancellationToken ct = default)
    {
        var shifts = await _uow.Shifts.SearchAsync(name, isActive, ct);
        return ApiResponse<List<ShiftDto>>.SuccessResponse(shifts.Select(s => s.ToDto()).ToList());
    }

    public async Task<ApiResponse<ShiftDto>> CreateAsync(CreateShiftDto dto, CancellationToken ct = default)
    {
        var error = Validate(dto, DefaultTimeZoneId, out var name, out var startTime, out var timeZoneId);
        if (error is not null)
            return ApiResponse<ShiftDto>.ErrorResponse(error);

        if (await _uow.Shifts.NameExistsAsync(name, null, ct))
            return ApiResponse<ShiftDto>.ErrorResponse($"A shift named '{name}' already exists.", 409);

        var shift = new Shift
        {
            Name            = name,
            StartTime       = startTime,
            DurationMinutes = dto.DurationMinutes,
            BreakMinutes    = dto.BreakMinutes,
            GraceMinutes    = dto.GraceMinutes,
            TimeZoneId      = timeZoneId,
            IsActive        = dto.IsActive ?? true,
            CreatedAt       = DateTimeOffset.UtcNow
        };

        await _uow.Shifts.AddAsync(shift);
        await _uow.SaveChangesAsync();

        return ApiResponse<ShiftDto>.CreatedResponse(shift.ToDto());
    }

    public async Task<ApiResponse<ShiftDto>> UpdateAsync(int id, UpdateShiftDto dto, CancellationToken ct = default)
    {
        var shift = await _uow.Shifts.GetByIdNotDeletedAsync(id, ct);
        if (shift is null)
            return ApiResponse<ShiftDto>.NotFoundResponse($"Shift {id} not found.");

        var error = Validate(dto, shift.TimeZoneId, out var name, out var startTime, out var timeZoneId);
        if (error is not null)
            return ApiResponse<ShiftDto>.ErrorResponse(error);

        if (await _uow.Shifts.NameExistsAsync(name, id, ct))
            return ApiResponse<ShiftDto>.ErrorResponse($"A shift named '{name}' already exists.", 409);

        shift.Name            = name;
        shift.StartTime       = startTime;
        shift.DurationMinutes = dto.DurationMinutes;
        shift.BreakMinutes    = dto.BreakMinutes;
        shift.GraceMinutes    = dto.GraceMinutes;
        shift.TimeZoneId      = timeZoneId;
        shift.IsActive        = dto.IsActive ?? shift.IsActive;
        shift.UpdatedAt       = DateTimeOffset.UtcNow;

        _uow.Shifts.Update(shift);
        await _uow.SaveChangesAsync();

        return ApiResponse<ShiftDto>.SuccessResponse(shift.ToDto());
    }

    private static string? Validate(CreateShiftDto dto, string fallbackTimeZoneId,
        out string name, out TimeOnly startTime, out string timeZoneId)
    {
        name       = dto.Name?.Trim() ?? string.Empty;
        startTime  = default;
        timeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? fallbackTimeZoneId : dto.TimeZoneId.Trim();

        if (name.Length == 0)
            return "Name is required.";
        if (name.Length > NameMaxLength)
            return $"Name must be at most {NameMaxLength} characters.";

        if (string.IsNullOrWhiteSpace(dto.StartTime) ||
            !TimeOnly.TryParseExact(dto.StartTime.Trim(), StartTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            return "StartTime is required and must be in \"HH:mm\" or \"HH:mm:ss\" format.";

        if (dto.DurationMinutes < 1 || dto.DurationMinutes > MaxDuration)
            return $"DurationMinutes must be between 1 and {MaxDuration}.";
        if (dto.BreakMinutes < 0 || dto.BreakMinutes >= dto.DurationMinutes)
            return "BreakMinutes must be 0 or more and less than DurationMinutes.";
        if (dto.GraceMinutes < 0 || dto.GraceMinutes > MaxGrace)
            return $"GraceMinutes must be between 0 and {MaxGrace}.";

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
            return $"TimeZoneId '{timeZoneId}' was not found.";

        return null;
    }
}