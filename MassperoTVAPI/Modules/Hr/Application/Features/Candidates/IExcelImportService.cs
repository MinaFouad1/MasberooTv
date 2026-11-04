
namespace MassperoTVAPI.Modules.Hr.Application.Features.Candidates;

public interface IExcelImportService
{
    Task<ImportResultDto> ImportCandidatesAsync(IFormFile file, string? userId = null);
    byte[] GenerateCandidateTemplate();
}
