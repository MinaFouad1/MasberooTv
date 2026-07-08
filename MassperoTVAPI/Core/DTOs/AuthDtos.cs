using System.ComponentModel.DataAnnotations;
using MassperoTVAPI.Core.Enums;

namespace MassperoTVAPI.Core.DTOs;

public record RegisterDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    public string UserName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; init; } = string.Empty;

    public string? Address { get; init; }

    /// <summary>Optional role to assign on registration (e.g. "Admin", "HR").</summary>
    public string? Role { get; init; }
}

public record LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; init; } = string.Empty;
}

public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
    /// <summary>UTC timestamp of this user's last successful login (null on first login).</summary>
    public DateTime? LastLoginAt { get; init; }
}

public record UpdateProfileDto
{
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; init; }

    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    public string? UserName { get; init; }

    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string? Address { get; init; }
}

public record ProfileDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
    public AccountStatus AccountStatus { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public int? CompanyId { get; init; }
    public string? Address { get; init; }
}
