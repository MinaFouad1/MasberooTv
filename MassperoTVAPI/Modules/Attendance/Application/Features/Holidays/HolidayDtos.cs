namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Holidays;

public record HolidayDto(
    int Id,
    string Name,
    DateOnly HolidayDate,
    bool IsPaid,
    string HolidayType
);

public record CreateHolidayDto
{
    public string? Name { get; init; }

    /// <summary>Date as "yyyy-MM-dd".</summary>
    public DateOnly? HolidayDate { get; init; }

    /// <summary>Defaults to true on create.</summary>
    public bool? IsPaid { get; init; }

    /// <summary>National or Company. Defaults to National on create.</summary>
    public string? HolidayType { get; init; }
}

public record UpdateHolidayDto : CreateHolidayDto;