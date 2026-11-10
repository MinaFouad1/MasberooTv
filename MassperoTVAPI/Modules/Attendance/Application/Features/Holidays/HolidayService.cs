using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Holidays;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

public class HolidayService : IHolidayService
{
    private const int NameMaxLength = 150;

    private readonly IUnitOfWork _uow;

    public HolidayService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<List<HolidayDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var holidays = await _uow.Holidays.GetAllOrderedAsync(ct);
        return ApiResponse<List<HolidayDto>>.SuccessResponse(holidays.Select(h => h.ToDto()).ToList());
    }

    public async Task<ApiResponse<HolidayDto>> CreateAsync(CreateHolidayDto dto, CancellationToken ct = default)
    {
        var error = Validate(dto, HolidayTypes.National, out var name, out var date, out var holidayType);
        if (error is not null)
            return ApiResponse<HolidayDto>.ErrorResponse(error);

        if (await _uow.Holidays.DateExistsAsync(date, null, ct))
            return ApiResponse<HolidayDto>.ErrorResponse($"A holiday already exists on {date:yyyy-MM-dd}.", 409);

        var holiday = new Holiday
        {
            Name        = name,
            HolidayDate = date,
            IsPaid      = dto.IsPaid ?? true,
            HolidayType = holidayType,
            CreatedAt   = DateTimeOffset.UtcNow
        };

        await _uow.Holidays.AddAsync(holiday);
        await _uow.SaveChangesAsync();

        return ApiResponse<HolidayDto>.CreatedResponse(holiday.ToDto());
    }

    public async Task<ApiResponse<HolidayDto>> UpdateAsync(int id, UpdateHolidayDto dto, CancellationToken ct = default)
    {
        var holiday = await _uow.Holidays.GetByIdNotDeletedAsync(id, ct);
        if (holiday is null)
            return ApiResponse<HolidayDto>.NotFoundResponse($"Holiday {id} not found.");

        var error = Validate(dto, holiday.HolidayType, out var name, out var date, out var holidayType);
        if (error is not null)
            return ApiResponse<HolidayDto>.ErrorResponse(error);

        if (await _uow.Holidays.DateExistsAsync(date, id, ct))
            return ApiResponse<HolidayDto>.ErrorResponse($"A holiday already exists on {date:yyyy-MM-dd}.", 409);

        holiday.Name        = name;
        holiday.HolidayDate = date;
        holiday.IsPaid      = dto.IsPaid ?? holiday.IsPaid;
        holiday.HolidayType = holidayType;
        holiday.UpdatedAt   = DateTimeOffset.UtcNow;

        _uow.Holidays.Update(holiday);
        await _uow.SaveChangesAsync();

        return ApiResponse<HolidayDto>.SuccessResponse(holiday.ToDto());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var holiday = await _uow.Holidays.GetByIdNotDeletedAsync(id, ct);
        if (holiday is null)
            return ApiResponse<bool>.NotFoundResponse($"Holiday {id} not found.");

        holiday.IsDeleted = true;
        holiday.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.Holidays.Update(holiday);
        await _uow.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Holiday deleted successfully.");
    }

    private static string? Validate(CreateHolidayDto dto, string fallbackHolidayType,
        out string name, out DateOnly date, out string holidayType)
    {
        name        = dto.Name?.Trim() ?? string.Empty;
        date        = dto.HolidayDate ?? default;
        holidayType = fallbackHolidayType;

        if (name.Length == 0)
            return "Name is required.";
        if (name.Length > NameMaxLength)
            return $"Name must be at most {NameMaxLength} characters.";
        if (dto.HolidayDate is null)
            return "HolidayDate is required (yyyy-MM-dd).";

        if (!string.IsNullOrWhiteSpace(dto.HolidayType))
        {
            var normalized = HolidayTypes.Normalize(dto.HolidayType);
            if (normalized is null)
                return $"HolidayType must be one of: {string.Join(", ", HolidayTypes.All)}.";
            holidayType = normalized;
        }
        else if (HolidayTypes.Normalize(holidayType) is null)
        {
            // Existing rows created before the column existed may hold an empty value.
            holidayType = HolidayTypes.National;
        }

        return null;
    }
}