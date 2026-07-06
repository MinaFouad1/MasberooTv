using System.ComponentModel.DataAnnotations;

namespace MassperoTVAPI.Core.DTOs;

public record CreateConfigurationDto
{
    [Required(ErrorMessage = "Key is required.")]
    [MaxLength(255, ErrorMessage = "Key cannot exceed 255 characters.")]
    public string Key { get; init; } = string.Empty;

    [Required(ErrorMessage = "Value (base URL) is required.")]
    public string Value { get; init; } = string.Empty;
}

public record UpdateConfigurationDto
{
    [MaxLength(255)]
    public string? Key { get; init; }

    public string? Value { get; init; }
}

public record ConfigurationDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

// ─── File Upload ───────────────────────────────────────────────────────────────

public record FileUploadResponseDto
{
    public string FileName { get; init; } = string.Empty;
    public string FullUrl { get; init; } = string.Empty;
    public string FolderKey { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
}
