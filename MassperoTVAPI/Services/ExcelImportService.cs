using System.Globalization;
using ClosedXML.Excel;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MassperoTVAPI.Services;

public class ExcelImportService : IExcelImportService
{
    private readonly IUnitOfWork _uow;

    private static readonly HashSet<string> AllowedExtensions = [".csv", ".xlsx"];

    private const int BatchSize = 100;

    public ExcelImportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ImportResultDto> ImportCandidatesAsync(IFormFile file, string? userId = null)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is empty or null.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException($"File type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

        List<CandidateImportRowDto> rows;

        if (ext == ".csv")
            rows = ParseCsv(file);
        else
            rows = ParseXlsx(file);

        return await ImportRowsAsync(rows, file.FileName, userId);
    }

    private static List<CandidateImportRowDto> ParseCsv(IFormFile file)
    {
        var rows = new List<CandidateImportRowDto>();

        using var reader = new StreamReader(file.OpenReadStream());

        var headerLine = reader.ReadLine();
        if (headerLine is null) return rows;

        var headers = ParseCsvLine(headerLine);
        var colMap = BuildColumnMap(headers);

        int lineNumber = 1;

        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);
            if (values.Count < 3) continue;

            var row = MapToRow(values, colMap, lineNumber);
            if (row is not null)
                rows.Add(row);
        }

        return rows;
    }

    private static List<CandidateImportRowDto> ParseXlsx(IFormFile file)
    {
        var rows = new List<CandidateImportRowDto>();

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var firstRow = ws.FirstRowUsed();
        if (firstRow is null) return rows;

        var headers = new List<string>();
        foreach (var cell in firstRow.Cells())
            headers.Add(cell.GetString().Trim());

        var colMap = BuildColumnMap(headers);

        var range = ws.RangeUsed();
        if (range is null) return rows;
        var usedRows = range.RowsUsed().Skip(1);
        int rowNumber = 1;

        foreach (var row in usedRows)
        {
            rowNumber++;
            var values = new List<string>();
            foreach (var cell in row.Cells())
                values.Add(cell.GetString().Trim());

            var dto = MapToRow(values, colMap, rowNumber);
            if (dto is not null)
                rows.Add(dto);
        }

        return rows;
    }

    private static Dictionary<string, int> BuildColumnMap(List<string> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            var key = headers[i].Trim();
            if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
                map[key] = i;
        }
        return map;
    }

    private static CandidateImportRowDto? MapToRow(List<string> values, Dictionary<string, int> colMap, int rowNumber)
    {
        if (!colMap.TryGetValue("Name", out var nameIdx) || nameIdx >= values.Count)
            return null;

        var name = values[nameIdx].Trim();
        if (string.IsNullOrWhiteSpace(name))
            return null;

        int? jobId = TryGetInt(values, colMap, "JobId");
        int? statusId = TryGetInt(values, colMap, "StatusId");
        int? securityClearanceId = TryGetInt(values, colMap, "SecurityClearanceId");

        return new CandidateImportRowDto
        {
            RowNumber = rowNumber,
            Name = name,
            JobId = jobId ?? 0,
            StatusId = statusId ?? 0,
            SecurityClearanceId = securityClearanceId ?? 0,
            CvFile = TryGetString(values, colMap, "CvFile"),
            ReasonOfAccept = TryGetString(values, colMap, "ReasonOfAccept"),
            ReasonOfReject = TryGetString(values, colMap, "ReasonOfReject"),
            Accepted = TryGetBool(values, colMap, "Accepted"),
            ApplicationUserId = TryGetString(values, colMap, "ApplicationUserId")
        };
    }

    private async Task<ImportResultDto> ImportRowsAsync(List<CandidateImportRowDto> rows, string fileName, string? userId = null)
    {
        var errors = new List<ImportErrorDto>();
        var validRows = new List<Candidate>();

        var existingJobIds = (await _uow.Jobs.GetAllAsync()).Select(j => j.Id).ToHashSet();
        var existingStatusIds = (await _uow.Statuses.GetAllAsync()).Select(s => s.Id).ToHashSet();
        var existingClearanceIds = (await _uow.SecurityClearances.GetAllAsync()).Select(c => c.Id).ToHashSet();

        foreach (var row in rows)
        {
            var rowErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(row.Name))
                rowErrors.Add("Name is required.");

            if (!existingJobIds.Contains(row.JobId))
                rowErrors.Add($"JobId {row.JobId} not found.");
            if (!existingStatusIds.Contains(row.StatusId))
                rowErrors.Add($"StatusId {row.StatusId} not found.");
            if (!existingClearanceIds.Contains(row.SecurityClearanceId))
                rowErrors.Add($"SecurityClearanceId {row.SecurityClearanceId} not found.");

            if (rowErrors.Count > 0)
            {
                errors.Add(new ImportErrorDto
                {
                    RowNumber = row.RowNumber,
                    Message = string.Join(" ", rowErrors)
                });
                continue;
            }

            validRows.Add(new Candidate
            {
                Name = row.Name,
                JobId = row.JobId,
                StatusId = row.StatusId,
                SecurityClearanceId = row.SecurityClearanceId,
                CvFile = row.CvFile,
                ReasonOfAccept = row.ReasonOfAccept,
                ReasonOfReject = row.ReasonOfReject,
                Accepted = row.Accepted,
                ApplicationUserId = row.ApplicationUserId
            });
        }

        for (int i = 0; i < validRows.Count; i += BatchSize)
        {
            var batch = validRows.Skip(i).Take(BatchSize);
            foreach (var candidate in batch)
            {
                await _uow.Candidates.AddAsync(candidate);
            }

            await _uow.SaveChangesAsync();
        }

        return new ImportResultDto
        {
            FileName = fileName,
            TotalRows = rows.Count,
            Imported = validRows.Count,
            Failed = errors.Count,
            Errors = errors
        };
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    private static int? TryGetInt(List<string> values, Dictionary<string, int> colMap, string colName)
    {
        if (!colMap.TryGetValue(colName, out var idx) || idx >= values.Count)
            return null;

        var val = values[idx].Trim();
        if (string.IsNullOrWhiteSpace(val)) return null;

        return int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static string? TryGetString(List<string> values, Dictionary<string, int> colMap, string colName)
    {
        if (!colMap.TryGetValue(colName, out var idx) || idx >= values.Count)
            return null;

        var val = values[idx].Trim();
        return string.IsNullOrWhiteSpace(val) ? null : val;
    }

    private static bool TryGetBool(List<string> values, Dictionary<string, int> colMap, string colName)
    {
        if (!colMap.TryGetValue(colName, out var idx) || idx >= values.Count)
            return false;

        var val = values[idx].Trim().ToLowerInvariant();
        return val is "true" or "yes" or "1" or "y";
    }
}
