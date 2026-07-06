using MassperoTVAPI.Core.DTOs;

namespace MassperoTVAPI.Services;

public interface IExcelImportService
{
    Task<ImportResultDto> ImportCandidatesAsync(IFormFile file, string? userId = null);
}
