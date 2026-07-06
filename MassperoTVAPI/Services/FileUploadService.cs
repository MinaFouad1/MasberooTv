
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MassperoTVAPI.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    /// <summary>Allowed file extensions.</summary>
    private static readonly HashSet<string> AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
         ".pdf", ".doc", ".docx", ".xls", ".xlsx",
         ".txt", ".csv", ".zip", ".rar", ".mp4", ".mp3"];

    /// <summary>Maximum file size: 20 MB.</summary>
    private const long MaxFileSizeBytes = 20L * 1024 * 1024;

    public FileUploadService(
        IUnitOfWork unitOfWork,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _env = env;
    }

    /// <inheritdoc/>
    public async Task<FileUploadResponseDto> UploadAsync(IFormFile file, string folderKey)
    {
        // ── 1. Validate file ───────────────────────────────────────────────────
        ValidateFile(file);

        // ── 2. Lookup base URL from Configuration table ────────────────────────
        var appConfig = await _unitOfWork.Configurations.GetByKeyAsync(folderKey)
            ?? throw new KeyNotFoundException(
                $"Configuration key '{folderKey}' not found. " +
                $"Add it via POST /api/configuration first.");

        // ── 3. Resolve physical base path from appsettings ─────────────────────
        //      FileUpload:PhysicalBasePath can be absolute (D:\Uploads) or
        //      relative to ContentRoot (e.g. "Uploads")
        var physicalBase = _config["FileUpload:PhysicalBasePath"] ?? "Uploads";

        if (!Path.IsPathRooted(physicalBase))
            physicalBase = Path.Combine(_env.ContentRootPath, physicalBase);

        // ── 4. Build folder path and ensure it exists ──────────────────────────
        var folderPath = Path.Combine(physicalBase, folderKey);
        Directory.CreateDirectory(folderPath);

        // ── 5. Generate unique filename ─────────────────────────────────────────
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPhysicalPath = Path.Combine(folderPath, fileName);

        // ── 6. Save file to disk ───────────────────────────────────────────────
        await using var stream = new FileStream(fullPhysicalPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);

        // ── 7. Build and return full URL ───────────────────────────────────────
        //      URL = Configuration.Value (trimmed) + "/" + fileName
        var fullUrl = $"{appConfig.Value.TrimEnd('/')}/{fileName}";

        return new FileUploadResponseDto
        {
            FileName = fileName,
            FullUrl = fullUrl,
            FolderKey = folderKey,
            FileSizeBytes = file.Length,
            ContentType = file.ContentType
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileUploadResponseDto>> UploadManyAsync(
        IEnumerable<IFormFile> files, string folderKey)
    {
        var results = new List<FileUploadResponseDto>();
        foreach (var file in files)
            results.Add(await UploadAsync(file, folderKey));
        return results;
    }

    /// <inheritdoc/>
    public bool DeletePhysicalFile(string physicalPath)
    {
        if (!File.Exists(physicalPath)) return false;
        File.Delete(physicalPath);
        return true;
    }


    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is empty or null.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException(
                $"File size ({file.Length / 1024 / 1024} MB) exceeds the " +
                $"maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException(
                $"File type '{ext}' is not allowed. " +
                $"Allowed types: {string.Join(", ", AllowedExtensions)}");
    }
}
