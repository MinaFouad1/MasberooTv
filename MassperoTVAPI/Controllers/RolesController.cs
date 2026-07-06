using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<Core.Entities.ApplicationUser> _userManager;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<Core.Entities.ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>List all available roles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = _roleManager.Roles
            .Select(r => new RoleDto(r.Id, r.Name!))
            .ToList();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(roles));
    }

    /// <summary>Get a single role by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return NotFound(ApiResponse<RoleDto>.NotFoundResponse("Role not found."));

        return Ok(ApiResponse<RoleDto>.SuccessResponse(new RoleDto(role.Id, role.Name!)));
    }

    /// <summary>Create a new role.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Validation failed."));

        if (await _roleManager.RoleExistsAsync(dto.Name))
            return Conflict(ApiResponse<RoleDto>.ErrorResponse($"Role '{dto.Name}' already exists.", 409));

        var role   = new IdentityRole(dto.Name.Trim());
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse(
                "Failed to create role.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        var created = new RoleDto(role.Id, role.Name!);
        return StatusCode(201, ApiResponse<RoleDto>.CreatedResponse(created, $"Role '{role.Name}' created."));
    }

    /// <summary>Update a role name.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(string id, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Validation failed."));

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return NotFound(ApiResponse<RoleDto>.NotFoundResponse("Role not found."));

        if (await _roleManager.RoleExistsAsync(dto.Name) && role.Name != dto.Name)
            return Conflict(ApiResponse<RoleDto>.ErrorResponse($"Role '{dto.Name}' already exists.", 409));

        role.Name = dto.Name.Trim();
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse(
                "Failed to update role.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<RoleDto>.SuccessResponse(new RoleDto(role.Id, role.Name!), "Role updated."));
    }

    /// <summary>Delete a role.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("Role not found."));

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to delete role.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Deleted succesffuly"));
    }

    // ── POST /api/roles/assign ────────────────────────────────────────────────
    /// <summary>Assign a role to a user.</summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> AssignRole([FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));

        if (!await _roleManager.RoleExistsAsync(dto.RoleName))
            return NotFound(ApiResponse<object>.NotFoundResponse($"Role '{dto.RoleName}' not found."));

        if (await _userManager.IsInRoleAsync(user, dto.RoleName))
            return BadRequest(ApiResponse<object>.ErrorResponse($"User already has role '{dto.RoleName}'."));

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to assign role.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, $"Role '{dto.RoleName}' assigned to user."));
    }

    // ── DELETE /api/roles/remove ──────────────────────────────────────────────
    /// <summary>Remove a role from a user.</summary>
    [HttpDelete("remove")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveRole([FromBody] RemoveRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
            return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));

        if (!await _userManager.IsInRoleAsync(user, dto.RoleName))
            return BadRequest(ApiResponse<object>.ErrorResponse($"User does not have role '{dto.RoleName}'."));

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to remove role.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, $"Role '{dto.RoleName}' removed from user."));
    }
}
 