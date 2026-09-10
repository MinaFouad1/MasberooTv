using System.Globalization;
using ClosedXML.Excel;
using MassperoTVAPI.Core.DTOs;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Enums;
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

        var lastColUsed = ws.LastColumnUsed()?.ColumnNumber() ?? firstRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        if (lastColUsed == 0) return rows;

        var headers = new List<string>();
        for (int col = 1; col <= lastColUsed; col++)
        {
            headers.Add(GetCellString(firstRow.Cell(col)));
        }

        var colMap = BuildColumnMap(headers);

        var range = ws.RangeUsed();
        if (range is null) return rows;
        var usedRows = range.RowsUsed().Skip(1);
        int rowNumber = 1;

        foreach (var row in usedRows)
        {
            rowNumber++;
            var values = new List<string>();
            for (int col = 1; col <= lastColUsed; col++)
            {
                values.Add(GetCellString(row.Cell(col)));
            }

            var dto = MapToRow(values, colMap, rowNumber);
            if (dto is not null)
                rows.Add(dto);
        }

        return rows;
    }

    private static string GetCellString(IXLCell cell)
    {
        if (cell == null || cell.IsEmpty())
            return string.Empty;

        return cell.Value.ToString().Trim();
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
        var name = TryGetString(values, colMap, "Name", "الاسم", "اسم المرشح");
        if (string.IsNullOrWhiteSpace(name))
            return null;

        int? jobId = TryGetInt(values, colMap, "JobId", "Job Id", "Job", "الوظيفة");
        int? statusId = TryGetInt(values, colMap, "StatusId", "Status Id", "Status", "الحالة");
        int? securityClearanceId = TryGetInt(values, colMap, "SecurityClearanceId", "Security Clearance Id", "SecurityClearance", "الموقف الأمني", "الموقف الامني");

        return new CandidateImportRowDto
        {
            RowNumber = rowNumber,
            Name = name,
            JobId = jobId ?? 0,
            StatusId = statusId ?? 0,
            SecurityClearanceId = securityClearanceId ?? 0,
            CvFile = TryGetString(values, colMap, "CvFile", "CV", "السيرة الذاتية"),
            ReasonOfAccept = TryGetString(values, colMap, "ReasonOfAccept", "سبب القبول"),
            ReasonOfReject = TryGetString(values, colMap, "ReasonOfReject", "سبب الرفض"),
            Accepted = TryGetBool(values, colMap, "Accepted", "مقبول"),
            ApplicationUserId = TryGetString(values, colMap, "ApplicationUserId", "UserId", "المستخدم"),
            Gender = TryGetGender(values, colMap, "Gender", "Sex", "النوع", "الجنس"),
            Email = TryGetString(values, colMap, "Email", "E-mail", "Mail", "البريد الإلكتروني", "البريد الالكتروني"),
            Address = TryGetString(values, colMap, "Address", "العنوان", "Location"),
            PhoneNumber = TryGetString(values, colMap, "PhoneNumber", "Phone", "Mobile", "Phone Number", "Mobile Number", "MobileNumber", "الهاتف", "الموبايل", "التليفون", "رقم الهاتف"),
            AlternatePhoneNumber = TryGetString(values, colMap, "AlternatePhoneNumber", "AlternateMobile", "AlternatePhone", "Phone2", "Mobile2", "هاتف بديل"),
            Nationality = TryGetString(values, colMap, "Nationality", "Natinality", "الجنسية", "القومية"),
            NationalId = TryGetString(values, colMap, "NationalId", "National ID", "NationalIdNumber", "الرقم القومي", "رقم البطاقة"),
            MaritalStatus = TryGetMaritalStatus(values, colMap, "MaritalStatus", "Marital Status", "mtrialstatus", "الحالة الاجتماعية"),
            DateOfBeginning = TryGetDateTime(values, colMap, "DateOfBeginning", "Date of Beginning", "Start Date", "StartDate", "تاريخ البدء"),
            HiringDate = TryGetDateTime(values, colMap, "ApplicationDate", "Application Date", "AppliedDate", "Applied Date", "HiringDate", "Hiring Date", "تاريخ التقديم"),
            PreferredJobLocation = TryGetString(values, colMap, "PreferredJobLocation", "Preferred Location", "PreferredLocation", "Preferred Work Location", "موقع العمل المفضل"),
            PreferredShift = TryGetPreferredShift(values, colMap, "PreferredShift", "Preferred Shift", "وردية العمل المفضلة"),
            City = TryGetString(values, colMap, "City", "المدينة", "المحافظة"),
            Country = TryGetCountry(values, colMap, "Country", "البلد", "الدولة"),
            CurrentEmployer = TryGetString(values, colMap, "CurrentEmployer", "Current Employer", "جهة العمل الحالية"),
            CurrentPosition = TryGetString(values, colMap, "CurrentPosition", "Current Position", "المسمى الوظيفي الحالي"),
            YearsOfExperience = TryGetInt(values, colMap, "YearsOfExperience", "Years of Experience", "yearsofexceince", "Years", "سنوات الخبرة"),
            ExpectedSalary = TryGetDecimal(values, colMap, "ExpectedSalary", "Expected Salary", "الراتب المتوقع"),
            CurrentSalary = TryGetDecimal(values, colMap, "CurrentSalary", "Current Salary", "الراتب الحالي"),
            NoticePeriod = TryGetString(values, colMap, "NoticePeriod", "Notice Period", "notice period", "فترة الإخطار", "فترة الاخطار"),
            Availability = TryGetString(values, colMap, "Availability", "فترة التوافر")
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
                ApplicationUserId = row.ApplicationUserId,
                Gender = row.Gender,
                Email = row.Email,
                Address = row.Address,
                Mobile = row.PhoneNumber,
                AlternateMobile = row.AlternatePhoneNumber,
                Nationality = row.Nationality,
                NationalId = row.NationalId,
                MaritalStatus = row.MaritalStatus,
                DateOfBeginning = row.DateOfBeginning,
                HiringDate = row.HiringDate,
                PreferredJobLocation = row.PreferredJobLocation,
                PreferredShift = row.PreferredShift,
                City = row.City,
                Country = row.Country,
                CurrentEmployer = row.CurrentEmployer,
                CurrentPosition = row.CurrentPosition,
                YearsOfExperience = row.YearsOfExperience,
                ExpectedSalary = row.ExpectedSalary,
                CurrentSalary = row.CurrentSalary,
                NoticePeriod = row.NoticePeriod,
                Availability = row.Availability
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

    public byte[] GenerateCandidateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Candidates");

        var headers = new[]
        {
            "Name",
            "JobId",
            "StatusId",
            "SecurityClearanceId",
            "Gender",
            "PhoneNumber",
            "Address",
            "Email",
            "Nationality",
            "NationalId",
            "MaritalStatus",
            "DateOfBeginning",
            "ApplicationDate",
            "PreferredLocation",
            "PreferredShift",
            "City",
            "Country",
            "CurrentEmployer",
            "CurrentPosition",
            "YearsOfExperience",
            "ExpectedSalary",
            "CurrentSalary",
            "NoticePeriod",
            "Availability",
            "CvFile",
            "ReasonOfAccept",
            "ReasonOfReject",
            "Accepted"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var sampleRows = new[]
        {
            new object[] {
                "أحمد محمود علي", 1, 1, 1, "Male", "01012345678", "القاهرة - مدينة نصر", "ahmed.ali@example.com", "Egyptian",
                "29801011234567", "Married", "2026-10-01", "2026-09-01", "Cairo", "Morning", "Cairo", "Egypt",
                "Al-Hayat TV", "Senior Presenter", 8, 25000m, 20000m, "1 Month", "Immediate", "cv_ahmed.pdf", "خبرة ممتازة", "", true
            },
            new object[] {
                "سارة محمد حسن", 2, 2, 2, "Female", "01123456789", "الجيزة - الدقي", "sara.hassan@example.com", "Egyptian",
                "29902021234568", "Single", "2026-10-15", "2026-09-05", "Giza", "Flexible", "Giza", "Egypt",
                "DMC Channel", "Production Assistant", 3, 12000m, 9500m, "2 Weeks", "After 2 weeks", "cv_sara.pdf", "", "", false
            },
            new object[] {
                "John Smith", 1002, 1, 1, "Male", "01234567890", "Alexandria - Smouha", "john.smith@example.com", "British",
                "GBR12345678", "Married", "2026-11-01", "2026-09-08", "Alexandria", "Morning", "Alexandria", "Other",
                "BBC Arabic", "Broadcast Engineer", 10, 45000m, 38000m, "1 Month", "1 Month", "cv_john.pdf", "Qualified specialist", "", true
            }
        };

        for (int r = 0; r < sampleRows.Length; r++)
        {
            var rowData = sampleRows[r];
            for (int c = 0; c < rowData.Length; c++)
            {
                var cell = ws.Cell(r + 2, c + 1);
                var val = rowData[c];
                if (val is int intVal) cell.Value = intVal;
                else if (val is decimal decVal) cell.Value = decVal;
                else if (val is bool boolVal) cell.Value = boolVal;
                else cell.Value = val?.ToString() ?? string.Empty;
            }
        }

        ws.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
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

    private static int? TryGetInt(List<string> values, Dictionary<string, int> colMap, params string[] columnNames)
    {
        foreach (var colName in columnNames)
        {
            if (!colMap.TryGetValue(colName, out var idx) || idx >= values.Count)
                continue;

            var val = values[idx].Trim();
            if (string.IsNullOrWhiteSpace(val)) continue;

            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return result;
        }

        return null;
    }

    private static decimal? TryGetDecimal(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var res) ? res : null;
    }

    private static DateTime? TryGetDateTime(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        if (DateTime.TryParse(value, out dt))
            return dt;

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var oaDate) && oaDate > 30000 && oaDate < 60000)
        {
            try { return DateTime.FromOADate(oaDate); } catch { }
        }

        return null;
    }

    private static string? TryGetString(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!colMap.TryGetValue(columnName, out var idx) || idx >= values.Count)
                continue;

            var value = values[idx].Trim();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static Gender? TryGetGender(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized is "male" or "m" or "1" or "ذكر" or "ولد" or "رجل")
            return Gender.Male;

        if (normalized is "female" or "f" or "2" or "أنثى" or "انثى" or "بنت" or "سيدة")
            return Gender.Female;

        return null;
    }

    private static MaritalStatus? TryGetMaritalStatus(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized is "single" or "1" or "اعزب" or "أعزب" or "انسة" or "آنسة")
            return MaritalStatus.Single;
        if (normalized is "married" or "2" or "متزوج" or "متزوجة")
            return MaritalStatus.Married;
        if (normalized is "divorced" or "3" or "مطلق" or "مطلقة")
            return MaritalStatus.Divorced;
        if (normalized is "widowed" or "4" or "ارمل" or "أرمل" or "ارملة" or "أرملة")
            return MaritalStatus.Widowed;

        return null;
    }

    private static Country? TryGetCountry(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<Country>(value, true, out var country))
            return country;

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized is "egypt" or "1" or "مصر") return Country.Egypt;
        if (normalized is "saudi arabia" or "saudi" or "ksa" or "2" or "السعودية") return Country.SaudiArabia;
        if (normalized is "uae" or "emirates" or "3" or "الامارات" or "الإمارات") return Country.UAE;

        return Country.Other;
    }

    private static PreferredShift? TryGetPreferredShift(
        List<string> values,
        Dictionary<string, int> colMap,
        params string[] columnNames)
    {
        var value = TryGetString(values, colMap, columnNames);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized is "morning" or "1" or "صباحي" or "صباحية") return PreferredShift.Morning;
        if (normalized is "evening" or "2" or "مسائي" or "مسائية") return PreferredShift.Evening;
        if (normalized is "night" or "3" or "ليلي" or "ليلية") return PreferredShift.Night;
        if (normalized is "flexible" or "4" or "مرن" or "مرنة") return PreferredShift.Flexible;

        return null;
    }

    private static bool TryGetBool(List<string> values, Dictionary<string, int> colMap, params string[] columnNames)
    {
        var val = TryGetString(values, colMap, columnNames)?.Trim().ToLowerInvariant();
        return val is "true" or "yes" or "1" or "y" or "نعم";
    }
}
