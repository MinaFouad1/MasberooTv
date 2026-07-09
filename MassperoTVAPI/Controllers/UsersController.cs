using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
using MassperoTVAPI.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Controllers;

/// <summary>
/// User management — Admin only.
/// GET    /api/users               → list all users (simple search)
/// GET    /api/users/search        → advanced search with AND/OR mode
/// GET    /api/users/{id}          → get user by ID
/// POST   /api/users/hr            → create a user with the HR role
/// PATCH  /api/users/{id}/status   → set AccountStatus (Active / Blocked / Locked)
/// DELETE /api/users/{id}          → delete a user
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole>    _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole>    roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserSearchResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserSearchResultDto>>>> Search(
        [FromQuery] UserSearchQueryDto query)
    {
        var isOr = query.Mode.Equals("OR", StringComparison.OrdinalIgnoreCase);

        var hasUserName = !string.IsNullOrWhiteSpace(query.UserName);
        var hasEmail    = !string.IsNullOrWhiteSpace(query.Email);
        var hasStatus   = query.Status.HasValue;
        // Role filter is applied in-memory (Identity doesn't expose role FK in Users table)
        var hasRole     = !string.IsNullOrWhiteSpace(query.Role);

        // No filters at all → return every user
        bool noFilters = !hasUserName && !hasEmail && !hasStatus && !hasRole;

        // ── DB-level filtering (username / email / status) ────────────────────
        IQueryable<ApplicationUser> dbQuery = _userManager.Users;

        if (!noFilters)
        {
            if (isOr)
            {
                // OR: at least one DB-level condition must match
                dbQuery = dbQuery.Where(u =>
                    (hasUserName && u.UserName!.Contains(query.UserName!)) ||
                    (hasEmail    && u.Email!.Contains(query.Email!))       ||
                    (hasStatus   && u.AccountStatus == query.Status!.Value));
            }
            else
            {
                // AND: every supplied DB-level condition must match
                if (hasUserName)
                    dbQuery = dbQuery.Where(u => u.UserName!.Contains(query.UserName!));
                if (hasEmail)
                    dbQuery = dbQuery.Where(u => u.Email!.Contains(query.Email!));
                if (hasStatus)
                    dbQuery = dbQuery.Where(u => u.AccountStatus == query.Status!.Value);
            }
        }

        var users = await dbQuery.ToListAsync();

        // ── In-memory role filtering ──────────────────────────────────────────
        var results = new List<UserSearchResultDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);

            if (hasRole)
            {
                bool roleMatch = roles.Contains(query.Role!, StringComparer.OrdinalIgnoreCase);

                if (isOr)
                {
                    // OR mode: if none of the DB filters already hit, skip unless role matches
                    bool dbHit = (hasUserName && u.UserName!.Contains(query.UserName!))  ||
                                 (hasEmail    && u.Email!.Contains(query.Email!))         ||
                                 (hasStatus   && u.AccountStatus == query.Status!.Value);
                    if (!dbHit && !roleMatch) continue;
                }
                else
                {
                    // AND mode: role MUST also match
                    if (!roleMatch) continue;
                }
            }

            results.Add(new UserSearchResultDto(
                u.Id,
                u.UserName!,
                u.Email!,
                roles,
                u.AccountStatus,
                u.LastLoginAt));
        }

        return Ok(ApiResponse<IEnumerable<UserSearchResultDto>>.SuccessResponse(
            results, $"{results.Count} user(s) found."));
    }

    // ── GET /api/users/statistics ─────────────────────────────────────────────
    /// <summary>Get statistics for all users including total counts and percentages.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<UserStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetStatistics()
    {
        var users = await _userManager.Users.ToListAsync();
        int total = users.Count;
        
        int activeCount = users.Count(u => u.AccountStatus == AccountStatus.Active);
        int lockedCount = users.Count(u => u.AccountStatus == AccountStatus.Locked);
        int blockedCount = users.Count(u => u.AccountStatus == AccountStatus.Blocked);
        int inactiveCount = lockedCount + blockedCount;
        
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        int adminCount = admins.Count;

        decimal CalcPercentage(int count) => total > 0 ? Math.Round((decimal)count / total * 100, 2) : 0;

        var stats = new UserStatisticsDto(
            total,
            new UserStatDto(activeCount, CalcPercentage(activeCount)),
            new UserStatDto(inactiveCount, CalcPercentage(inactiveCount)),
            new UserStatDto(adminCount, CalcPercentage(adminCount)),
            new UserStatDto(lockedCount, CalcPercentage(lockedCount)),
            new UserStatDto(blockedCount, CalcPercentage(blockedCount))
        );

        return Ok(ApiResponse<UserStatisticsDto>.SuccessResponse(stats));
    }

    // ── GET /api/users/{id} ───────────────────────────────────────────────────
    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.NotFoundResponse("User not found."));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<UserDto>.SuccessResponse(
            new UserDto(user.Id, user.UserName!,user.PhoneNumber!, user.Email!, user.IsVerified ,user.FailedLoginAttempts, roles, user.AccountStatus, user.LastLoginAt)));
    }

    // ── POST /api/users/hr ────────────────────────────────────────────────────
    /// <summary>Create a new user and automatically assign the HR role.</summary>
    [HttpPost("hr")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateHr([FromBody] CreateHrUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errs = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Validation failed.", 400, errs));
        }

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(ApiResponse<UserDto>.ErrorResponse("Email is already registered.", 409));

        if (await _userManager.FindByNameAsync(dto.UserName) is not null)
            return Conflict(ApiResponse<UserDto>.ErrorResponse("Username is already taken.", 409));

        var user = new ApplicationUser
        {
            Email    = dto.Email.Trim(),
            UserName = dto.UserName.Trim(),
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(
                "Failed to create user.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        // Ensure HR role exists then assign it
        if (!await _roleManager.RoleExistsAsync("HR"))
            await _roleManager.CreateAsync(new IdentityRole("HR"));

        await _userManager.AddToRoleAsync(user, "HR");
        var roles = await _userManager.GetRolesAsync(user);

        return StatusCode(201, ApiResponse<UserDto>.CreatedResponse(
            new UserDto(user.Id, user.UserName!,user.PhoneNumber!, user.Email!, user.IsVerified, user.FailedLoginAttempts, roles, user.AccountStatus, user.LastLoginAt),
            "HR user created successfully."));
    }

    // ── PATCH /api/users/{id}/status ─────────────────────────────────────────
    /// <summary>
    /// Set the account status of a user (Active / Blocked / Locked).
    /// Setting to Active also resets the failed login attempt counter.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateStatus(string id, [FromBody] UpdateUserStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errs = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Validation failed.", 400, errs));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.NotFoundResponse("User not found."));

        user.AccountStatus = dto.Status;

        // Reset lockout counter whenever an admin explicitly sets the account to Active
        if (dto.Status == AccountStatus.Active)
            user.FailedLoginAttempts = 0;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(
                "Failed to update user status.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<UserDto>.SuccessResponse(
            new UserDto(user.Id, user.UserName!,user.PhoneNumber!, user.Email!, user.IsVerified, user.FailedLoginAttempts, roles, user.AccountStatus, user.LastLoginAt),
            $"User status updated to '{dto.Status}' successfully."));
    }

    // ── DELETE /api/users/{id} ────────────────────────────────────────────────
    /// <summary>Delete a user account.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to delete user.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted successfully."));
    }

    // ── POST /api/users/{id}/reset-password ───────────────────────────────────
    /// <summary>Reset a user's password (Admin only).</summary>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] AdminResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errs = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed.", 400, errs));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to reset password.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password reset successfully."));
    }

    // ── PUT /api/users/{id} ───────────────────────────────────────────────────
    /// <summary>Edit user details (Admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] AdminUpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errs = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed.", 400, errs));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingByEmail is not null)
                return Conflict(ApiResponse<object>.ErrorResponse("Email is already registered by another user.", 409));
        }

        if (!string.Equals(user.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var existingByName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingByName is not null)
                return Conflict(ApiResponse<object>.ErrorResponse("Username is already taken by another user.", 409));
        }

        user.Email = dto.Email;
        user.UserName = dto.UserName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to update user.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<UserDto>.SuccessResponse(
            new UserDto(user.Id, user.UserName!, user.PhoneNumber!, user.Email!, user.IsVerified, user.FailedLoginAttempts, roles, user.AccountStatus, user.LastLoginAt),
            "User updated successfully."));
    }
}
