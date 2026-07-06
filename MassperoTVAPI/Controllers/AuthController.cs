using System.Security.Claims;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

  

    // ── POST api/auth/register ────────────────────────────────────────────────
    /// <summary>Register a new user. Optionally assign a role (Admin only).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed.", 400, errors));
        }

        // Duplicate email guard
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(ApiResponse<AuthResponseDto>.ErrorResponse("Email is already registered.", 409));

        // Duplicate username guard
        if (await _userManager.FindByNameAsync(dto.UserName) is not null)
            return Conflict(ApiResponse<AuthResponseDto>.ErrorResponse("Username is already taken.", 409));

        var user = new ApplicationUser
        {
            Email    = dto.Email.Trim(),
            UserName = dto.UserName.Trim(),
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                "Registration failed.", 400,
                result.Errors.Select(e => e.Description).ToList()));

        // Assign role if provided and it exists
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse($"Role '{dto.Role}' does not exist."));

            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var token = await _tokenService.CreateTokenAsync(user);
        var roles  = await _userManager.GetRolesAsync(user);

        return StatusCode(201, ApiResponse<AuthResponseDto>.CreatedResponse(new AuthResponseDto
        {
            Token    = token,
            Email    = user.Email!,
            UserName = user.UserName!,
            Roles    = roles.ToList()
        }, "User registered successfully."));
    } 

    /// <summary>Authenticate a user with email and password. Returns a JWT token on success.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password."));

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password."));

        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserName = user.UserName!,
            Roles = roles.ToList()
        }, "Login successful."));
    }

    /// <summary>Get current authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null)
            return NotFound(ApiResponse<AuthResponseDto>.NotFoundResponse("User not found."));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(new AuthResponseDto
        {
            Token = string.Empty,
            Email = user.Email!,
            UserName = user.UserName!,
            Roles = roles.ToList()
        }));
    }

    /// <summary>Update current authenticated user's profile.</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return BadRequest(ApiResponse<ProfileDto>.ErrorResponse("Validation failed.", 400, errors));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ApiResponse<ProfileDto>.UnauthorizedResponse("User not found."));

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound(ApiResponse<ProfileDto>.NotFoundResponse("User not found."));

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim();
            var existingEmailUser = await _userManager.FindByEmailAsync(email);
            if (existingEmailUser is not null && existingEmailUser.Id != user.Id)
                return Conflict(ApiResponse<ProfileDto>.ErrorResponse("Email is already registered.", 409));

            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(dto.UserName))
        {
            var userName = dto.UserName.Trim();
            var existingUserName = await _userManager.FindByNameAsync(userName);
            if (existingUserName is not null && existingUserName.Id != user.Id)
                return Conflict(ApiResponse<ProfileDto>.ErrorResponse("Username is already registered.", 409));

            user.UserName = userName;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(ApiResponse<ProfileDto>.ErrorResponse(
                "Profile update failed.", 400, updateResult.Errors.Select(e => e.Description).ToList()));

        //var company = await _unitOfWork.Companies.GetByUserIdAsync(user.Id);
        //if (company is not null && dto.Address is not null)
        //{
        //    company.Address = dto.Address.Trim();
        //    _unitOfWork.Companies.Update(company);
        //    await _unitOfWork.SaveChangesAsync();
        //}

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(new ProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            //CompanyId = company?.Id,
            //Address = company?.Address
        }, "Profile updated successfully."));
    }
}
