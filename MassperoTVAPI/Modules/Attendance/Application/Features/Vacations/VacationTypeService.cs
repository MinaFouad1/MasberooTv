using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Vacations;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Vacations;

public class VacationTypeService : IVacationTypeService
{
    private const int NameMaxLength = 100;

    private readonly IUnitOfWork _uow;

    public VacationTypeService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<List<VacationTypeDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var types = await _uow.VacationTypes.GetAllOrderedAsync(ct);
        return ApiResponse<List<VacationTypeDto>>.SuccessResponse(types.Select(t => t.ToDto()).ToList());
    }

    public async Task<ApiResponse<VacationTypeDto>> CreateAsync(CreateVacationTypeDto dto, CancellationToken ct = default)
    {
        var error = Validate(dto, out var name);
        if (error is not null)
            return ApiResponse<VacationTypeDto>.ErrorResponse(error);

        if (await _uow.VacationTypes.NameExistsAsync(name, null, ct))
            return ApiResponse<VacationTypeDto>.ErrorResponse($"A vacation type named '{name}' already exists.", 409);

        var type = new VacationType
        {
            Name         = name,
            IsPaid       = dto.IsPaid,
            DefaultDays  = dto.DefaultDays,
            CanCarryOver = dto.CanCarryOver,
            CreatedAt    = DateTimeOffset.UtcNow
        };

        await _uow.VacationTypes.AddAsync(type);
        await _uow.SaveChangesAsync();

        return ApiResponse<VacationTypeDto>.CreatedResponse(type.ToDto());
    }

    public async Task<ApiResponse<VacationTypeDto>> UpdateAsync(int id, UpdateVacationTypeDto dto, CancellationToken ct = default)
    {
        var type = await _uow.VacationTypes.GetByIdNotDeletedAsync(id, ct);
        if (type is null)
            return ApiResponse<VacationTypeDto>.NotFoundResponse($"Vacation type {id} not found.");

        var error = Validate(dto, out var name);
        if (error is not null)
            return ApiResponse<VacationTypeDto>.ErrorResponse(error);

        if (await _uow.VacationTypes.NameExistsAsync(name, id, ct))
            return ApiResponse<VacationTypeDto>.ErrorResponse($"A vacation type named '{name}' already exists.", 409);

        // Changing DefaultDays does not touch existing balances.
        type.Name         = name;
        type.IsPaid       = dto.IsPaid;
        type.DefaultDays  = dto.DefaultDays;
        type.CanCarryOver = dto.CanCarryOver;
        type.UpdatedAt    = DateTimeOffset.UtcNow;

        _uow.VacationTypes.Update(type);
        await _uow.SaveChangesAsync();

        return ApiResponse<VacationTypeDto>.SuccessResponse(type.ToDto());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var type = await _uow.VacationTypes.GetByIdNotDeletedAsync(id, ct);
        if (type is null)
            return ApiResponse<bool>.NotFoundResponse($"Vacation type {id} not found.");

        if (await _uow.VacationTypes.IsInUseAsync(id, ct))
            return ApiResponse<bool>.ErrorResponse(
                $"Vacation type '{type.Name}' is used by vacation requests or balances and cannot be deleted.", 409);

        type.IsDeleted = true;
        type.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.VacationTypes.Update(type);
        await _uow.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Vacation type deleted successfully.");
    }

    private static string? Validate(CreateVacationTypeDto dto, out string name)
    {
        name = dto.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
            return "Name is required.";
        if (name.Length > NameMaxLength)
            return $"Name must be at most {NameMaxLength} characters.";
        if (dto.DefaultDays < 0)
            return "DefaultDays must be 0 or more.";

        return null;
    }
}