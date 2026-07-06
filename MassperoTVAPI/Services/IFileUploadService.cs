using MassperoTVAPI.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace MassperoTVAPI.Services;

public interface IFileUploadService
{
    /// <summary>
    /// Upload a single file.
    /// Saves it to: {appsettings.FileUpload:PhysicalBasePath}/{folderKey}/{guid}.{ext}
    /// Returns full URL: Configuration[folderKey].Value + "/" + fileName
    /// </summary>
    Task<FileUploadResponseDto> UploadAsync(IFormFile file, string folderKey);

    /// <summary>Upload multiple files under the same folder key.</summary>
    Task<IEnumerable<FileUploadResponseDto>> UploadManyAsync(IEnumerable<IFormFile> files, string folderKey);

    /// <summary>Delete a file by its physical path. Returns false if file not found.</summary>
    bool DeletePhysicalFile(string physicalPath);
}
