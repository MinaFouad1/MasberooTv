using MassperoTVAPI.Modules.AttendanceModule.Domain.Enums;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

// -- Vacation types ---------------------------------------------------------
public record VacationTypeDto(
    int Id,
    string Name,
    bool IsPaid,
    int DefaultDays,
    bool CanCarryOver
);

public record CreateVacationTypeDto
{
    public string? Name { get; init; }
    public bool IsPaid { get; init; }
    public int DefaultDays { get; init; }
    public bool CanCarryOver { get; init; }
}

public record UpdateVacationTypeDto : CreateVacationTypeDto;

// -- Vacation balances ------------------------------------------------------
public record VacationBalanceDto(
    int VacationTypeId,
    string VacationTypeName,
    int Year,
    decimal Entitled,
    decimal CarriedOver,
    decimal Used,
    decimal Remaining
);

public record CreateVacationBalanceItemDto
{
    public int VacationTypeId { get; init; }

    /// <summary>Optional. Defaults to the vacation type's DefaultDays.</summary>
    public decimal? Entitled { get; init; }

    /// <summary>Optional. Defaults to 0.</summary>
    public decimal? CarriedOver { get; init; }
}

public record CreateVacationBalancesDto
{
    public int Year { get; init; }
    public List<CreateVacationBalanceItemDto>? Items { get; init; }
}

// -- Vacation requests ------------------------------------------------------
public record VacationRequestDto(
    int Id,
    int EmployeeId,
    int VacationTypeId,
    string VacationTypeName,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal Days,
    RequestStatus Status,
    string Reason,
    DateTimeOffset CreatedAt
);

public record CreateVacationRequestDto
{
    public int EmployeeId { get; init; }
    public int VacationTypeId { get; init; }

    /// <summary>Date as "yyyy-MM-dd".</summary>
    public DateOnly? FromDate { get; init; }

    /// <summary>Date as "yyyy-MM-dd".</summary>
    public DateOnly? ToDate { get; init; }

    public string? Reason { get; init; }
}