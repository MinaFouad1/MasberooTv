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
/// GET    /api/users              → list / search all users
/// GET    /api/users/{id}         → get user by ID
/// POST   /api/users/hr           → create a user with the HR role
/// PATCH  /api/users/{id}/status  → set AccountStatus (Active / Blocked / Locked)
/// DELETE /api/users/{id}         → delete a user
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

    // ── GET /api/users?search= ────────────────────────────────────────────────
    /// <summary>List all users. Optional search by username or email.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll(
        [FromQuery] string? search)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.UserName!.Contains(search) ||
                u.Email!.Contains(search));

        var users = await query.ToListAsync();

        var dtos = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserDto(u.Id, u.UserName!, u.Email!, u.IsVerified, roles, u.AccountStatus, u.LastLoginAt));
        }

        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(dtos));
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
            new UserDto(user.Id, user.UserName!, user.Email!, user.IsVerified, roles, user.AccountStatus, user.LastLoginAt)));
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
            new UserDto(user.Id, user.UserName!, user.Email!, user.IsVerified, roles, user.AccountStatus, user.LastLoginAt),
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
            new UserDto(user.Id, user.UserName!, user.Email!, user.IsVerified, roles, user.AccountStatus, user.LastLoginAt),
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
}

