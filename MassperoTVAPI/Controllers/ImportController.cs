using System.Security.Claims;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Helpers;
using MassperoTVAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassperoTVAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "HR,Admin")]
public class ImportController : ControllerBase
{
    private readonly IExcelImportService _importService;

    public ImportController(IExcelImportService importService)
        => _importService = importService;

    /// <summary>Import candidates from an Excel (.xlsx) or CSV file. Each imported candidate gets the default Pending process status and is logged.</summary>
    /// <param name="file">The Excel or CSV file to import.</param>
    /// <returns>Import result with counts of imported and failed rows, plus error details.</returns>
    [HttpPost("candidates")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportCandidates(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<ImportResultDto>.ErrorResponse("No file uploaded."));

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _importService.ImportCandidatesAsync(file, userId);
            return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result,
                $"Imported {result.Imported} of {result.TotalRows} candidates."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ImportResultDto>.ErrorResponse(ex.Message));
        }
    }
}
