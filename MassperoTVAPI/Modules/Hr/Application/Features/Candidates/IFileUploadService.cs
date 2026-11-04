using Microsoft.AspNetCore.Http;
using MassperoTVAPI.Modules.Hr.Application.Features.Configurations;

namespace MassperoTVAPI.Modules.Hr.Application.Features.Candidates;

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
