using MassperoTVAPI.Modules.AttendanceModule.Application.Abstractions;
using MassperoTVAPI.Modules.AttendanceModule.Domain.Entities.Permissions;
using MassperoTVAPI.Shared.Application;
using MassperoTVAPI.Shared.Persistence;

namespace MassperoTVAPI.Modules.AttendanceModule.Application.Features.Permissions;

public class PermissionTypeService : IPermissionTypeService
{
    private const int    NameMaxLength            = 100;  // column length
    private const int    MaxDefaultMonthlyMinutes = 1440;
    private const string ConcurrencyErrorMessage  = "Permission type changed, please retry.";

    private readonly IUnitOfWork               _uow;
    private readonly IConflictAwareSaveChanges _saveChanges;

    public PermissionTypeService(IUnitOfWork uow, IConflictAwareSaveChanges saveChanges)
    {
        _uow         = uow;
        _saveChanges = saveChanges;
    }

    public async Task<ApiResponse<List<PermissionTypeDto>>> GetAllAsync(bool? isActive, CancellationToken ct = default)
        => ApiResponse<List<PermissionTypeDto>>.SuccessResponse(await _uow.PermissionTypes.SearchAsync(isActive, ct));

    public async Task<ApiResponse<PermissionTypeDto>> CreateAsync(CreatePermissionTypeDto dto, CancellationToken ct = default)
    {
        var error = Validate(dto, out var name, out var defaultMonthlyMinutes, out var isActive);
        if (error is not null)
            return ApiResponse<PermissionTypeDto>.ErrorResponse(error);

        if (await _uow.PermissionTypes.NameExistsAsync(name, null, ct))
            return ApiResponse<PermissionTypeDto>.ErrorResponse($"A permission type named '{name}' already exists.", 409);

        var type = new PermissionType
        {
            Name                  = name,
            DefaultMonthlyMinutes = defaultMonthlyMinutes,
            IsActive              = isActive,
            CreatedAt             = DateTimeOffset.UtcNow
        };

        await _uow.PermissionTypes.AddAsync(type);
        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<PermissionTypeDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<PermissionTypeDto>.CreatedResponse(type.ToDto());
    }

    public async Task<ApiResponse<PermissionTypeDto>> UpdateAsync(int id, UpdatePermissionTypeDto dto, CancellationToken ct = default)
    {
        var type = await _uow.PermissionTypes.GetByIdNotDeletedAsync(id, ct);
        if (type is null)
            return ApiResponse<PermissionTypeDto>.NotFoundResponse($"Permission type {id} not found.");

        var error = Validate(dto, out var name, out var defaultMonthlyMinutes, out var isActive);
        if (error is not null)
            return ApiResponse<PermissionTypeDto>.ErrorResponse(error);

        if (await _uow.PermissionTypes.NameExistsAsync(name, id, ct))
            return ApiResponse<PermissionTypeDto>.ErrorResponse($"A permission type named '{name}' already exists.", 409);

        // DefaultMonthlyMinutes is informational: existing balances are not touched.
        type.Name                  = name;
        type.DefaultMonthlyMinutes = defaultMonthlyMinutes;
        type.IsActive              = isActive;
        type.UpdatedAt             = DateTimeOffset.UtcNow;

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<PermissionTypeDto>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<PermissionTypeDto>.SuccessResponse(type.ToDto());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var type = await _uow.PermissionTypes.GetByIdNotDeletedAsync(id, ct);
        if (type is null)
            return ApiResponse<bool>.NotFoundResponse($"Permission type {id} not found.");

        if (await _uow.PermissionTypes.IsInUseAsync(id, ct))
            return ApiResponse<bool>.ErrorResponse(
                $"Permission type '{type.Name}' is used by permission requests or balances and cannot be deleted.", 409);

        type.IsDeleted = true;
        type.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _saveChanges.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return ApiResponse<bool>.ErrorResponse(ConcurrencyErrorMessage, 409);
        }

        return ApiResponse<bool>.SuccessResponse(true, "Permission type deleted successfully.");
    }

    private static string? Validate(CreatePermissionTypeDto dto, out string name, out int defaultMonthlyMinutes, out bool isActive)
    {
        name                  = dto.Name?.Trim() ?? string.Empty;
        defaultMonthlyMinutes = dto.DefaultMonthlyMinutes ?? 0;
        isActive              = dto.IsActive ?? true;

        if (name.Length == 0)
            return "Name is required.";
        if (name.Length > NameMaxLength)
            return $"Name must be at most {NameMaxLength} characters.";
        if (defaultMonthlyMinutes < 0 || defaultMonthlyMinutes > MaxDefaultMonthlyMinutes)
            return $"DefaultMonthlyMinutes must be between 0 and {MaxDefaultMonthlyMinutes}.";
        return null;
    }
}
