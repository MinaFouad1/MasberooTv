using System.Text.Json;
using System.Text.Json.Serialization;
using MassperoTVAPI.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MassperoTVAPI.Core.Helpers;

/// <summary>
/// Custom model binder that binds a List of T from multipart/form-data:
/// 1. A JSON array string in form-data (e.g. Certifications: "[{\"name\":\"AWS\",\"url\":\"...\"}]")
/// 2. Alternate JSON field names (e.g. certificationsJson, CertificationsJson, dto.Certifications)
/// 3. Comma-separated plain strings (e.g. Certifications: "AWS, PMP")
/// 4. Standard indexed form fields (e.g. Certifications[0].Name, certifications[0][name])
/// </summary>
public class JsonOrFormListModelBinder<T> : IModelBinder where T : class, new()
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var propName = bindingContext.FieldName;
        if (string.IsNullOrWhiteSpace(propName))
        {
            propName = bindingContext.ModelName;
            if (propName.Contains('.'))
                propName = propName[(propName.LastIndexOf('.') + 1)..];
        }

        string[] rootNames = typeof(T) == typeof(UpsertCandidateCertificationDto)
            ? new[] { "Certifications", "Certification", "Certificates", "Certificate" }
            : typeof(T) == typeof(UpsertCandidateEducationDto)
                ? new[] { "Educations", "Education" }
                : new[] { propName };

        var request = bindingContext.HttpContext.Request;
        bool hadMatchingKey = false;
        var rawValues = new List<string>();

        // ── 1. Check Request.Form (primary for multipart/form-data) ───────────
        if (request.HasFormContentType && request.Form != null)
        {
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in request.Form.Keys)
            {
                var cleanKey = NormalizeKey(key);

                foreach (var root in rootNames)
                {
                    if (string.Equals(cleanKey, root, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(cleanKey, $"{root}Json", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(cleanKey, $"{root}String", StringComparison.OrdinalIgnoreCase))
                    {
                        if (processedKeys.Add(key))
                        {
                            hadMatchingKey = true;
                            foreach (var val in request.Form[key])
                            {
                                if (!string.IsNullOrWhiteSpace(val) && !val.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
                                {
                                    rawValues.Add(val.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            // ── 2. Fallback to ValueProvider if Request.Form not available ────
            foreach (var root in rootNames)
            {
                var candidatesToTry = new[]
                {
                    root,
                    $"dto.{root}",
                    $"{root}Json",
                    $"dto.{root}Json"
                };

                foreach (var key in candidatesToTry)
                {
                    var valResult = bindingContext.ValueProvider.GetValue(key);
                    if (valResult != ValueProviderResult.None)
                    {
                        hadMatchingKey = true;
                        foreach (var val in valResult)
                        {
                            if (!string.IsNullOrWhiteSpace(val) && !val.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
                            {
                                rawValues.Add(val.Trim());
                            }
                        }
                    }
                }
            }
        }

        var result = new List<T>();

        // ── 3. Parse collected raw string values ──────────────────────────────
        foreach (var rawValue in rawValues)
        {
            if (string.IsNullOrWhiteSpace(rawValue) || rawValue.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
                continue;

            // Case A: JSON Array "[{...}, {...}]"
            if (rawValue.StartsWith('[') && rawValue.EndsWith(']'))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<T>>(rawValue, JsonOptions);
                    if (items != null)
                    {
                        result.AddRange(items.Where(IsValidItem));
                        continue;
                    }
                }
                catch { }

                if (typeof(T) == typeof(UpsertCandidateCertificationDto))
                {
                    try
                    {
                        var strList = JsonSerializer.Deserialize<List<string>>(rawValue, JsonOptions);
                        if (strList is { Count: > 0 })
                        {
                            result.AddRange(strList
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Select(s => new UpsertCandidateCertificationDto { Name = s.Trim() })
                                .Cast<T>());
                            continue;
                        }
                    }
                    catch { }
                }
            }

            // Case B: Single JSON Object "{...}"
            if (rawValue.StartsWith('{') && rawValue.EndsWith('}'))
            {
                try
                {
                    var item = JsonSerializer.Deserialize<T>(rawValue, JsonOptions);
                    if (item != null && IsValidItem(item))
                    {
                        result.Add(item);
                        continue;
                    }
                }
                catch { }
            }

            // Case C: Comma-separated strings (for Certifications)
            if (typeof(T) == typeof(UpsertCandidateCertificationDto))
            {
                var certs = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(c => !string.IsNullOrWhiteSpace(c) && !c.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new UpsertCandidateCertificationDto { Name = c.Trim() })
                    .Cast<T>()
                    .ToList();

                if (certs.Count > 0)
                {
                    result.AddRange(certs);
                }
            }
        }

        // ── 4. If no items from raw strings, check indexed form keys ──────────
        if (result.Count == 0 && request.HasFormContentType && request.Form != null)
        {
            var indexed = ParseIndexedFormCollection(request.Form, rootNames);
            if (indexed != null && indexed.Count > 0)
            {
                hadMatchingKey = true;
                result.AddRange(indexed);
            }
        }

        // ── 5. Deduplicate items ──────────────────────────────────────────────
        if (typeof(T) == typeof(UpsertCandidateCertificationDto))
        {
            result = result
                .Cast<UpsertCandidateCertificationDto>()
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .Cast<T>()
                .ToList();
        }

        // ── 6. Set model binding result ───────────────────────────────────────
        if (result.Count > 0)
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else if (hadMatchingKey)
        {
            // The key was sent (e.g. Certifications = "[]" or empty), bind as empty list
            bindingContext.Result = ModelBindingResult.Success(new List<T>());
        }
        else
        {
            // Omitted entirely from request -> keep null so existing data is not erased
            bindingContext.Result = ModelBindingResult.Success(null);
        }

        return Task.CompletedTask;
    }

    private static bool IsValidItem(T item)
    {
        if (item is UpsertCandidateCertificationDto cert)
            return !string.IsNullOrWhiteSpace(cert.Name);
        if (item is UpsertCandidateEducationDto edu)
            return !string.IsNullOrWhiteSpace(edu.Degree) || !string.IsNullOrWhiteSpace(edu.University);
        return true;
    }

    public static List<T>? ParseIndexedFormCollection(IFormCollection form, params string[] prefixes)
    {
        var dict = new SortedDictionary<int, Dictionary<string, string>>();

        foreach (var rawKey in form.Keys)
        {
            var key = NormalizeKey(rawKey);

            foreach (var prefix in prefixes)
            {
                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var remainder = key[prefix.Length..];
                int index = -1;
                string propPart = string.Empty;

                if (remainder.StartsWith('['))
                {
                    var close = remainder.IndexOf(']');
                    if (close > 1 && int.TryParse(remainder[1..close], out var idx))
                    {
                        index = idx;
                        propPart = remainder[(close + 1)..].TrimStart('.', '[', ']').TrimEnd(']');
                    }
                }
                else if (remainder.StartsWith('.'))
                {
                    var parts = remainder.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[0], out var idx))
                    {
                        index = idx;
                        propPart = parts[1];
                    }
                }

                if (index < 0 || string.IsNullOrWhiteSpace(propPart))
                    continue;

                var value = form[rawKey].ToString();
                if (string.IsNullOrWhiteSpace(value) || value.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!dict.TryGetValue(index, out var propDict))
                {
                    propDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    dict[index] = propDict;
                }

                propDict[propPart] = value.Trim();
                break;
            }
        }

        if (dict.Count == 0)
            return null;

        var result = new List<T>();
        foreach (var (_, propDict) in dict)
        {
            try
            {
                var json = JsonSerializer.Serialize(propDict);
                var obj = JsonSerializer.Deserialize<T>(json, JsonOptions);
                if (obj != null && IsValidItem(obj))
                {
                    result.Add(obj);
                }
            }
            catch
            {
                // Ignore malformed entries
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static string NormalizeKey(string key)
    {
        var clean = key;
        if (clean.StartsWith("dto.", StringComparison.OrdinalIgnoreCase))
            clean = clean[4..];
        else if (clean.StartsWith("dto[", StringComparison.OrdinalIgnoreCase) && clean.IndexOf(']') > 4)
        {
            var close = clean.IndexOf(']');
            clean = clean[4..close].Trim('\'', '"') + clean[(close + 1)..];
        }
        return clean;
    }
}

/// <summary>
/// Custom model binder for List&lt;int&gt; (e.g. SkillIds, LanguageIds) from multipart/form-data:
/// JSON array "[1, 2, 3]", comma-separated "1, 2, 3", multiple keys "skillIds=1&amp;skillIds=2",
/// bracketed keys "skillIds[]=1", or indexed form fields "skillIds[0]=1".
/// </summary>
public class JsonOrFormIntListModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var propName = bindingContext.FieldName;
        if (string.IsNullOrWhiteSpace(propName))
        {
            propName = bindingContext.ModelName;
            if (propName.Contains('.'))
                propName = propName[(propName.LastIndexOf('.') + 1)..];
        }

        string[] rootNames = propName.Contains("Lang", StringComparison.OrdinalIgnoreCase)
            ? new[] { "LanguageIds", "LanguagesId", "LanguageId", "Languages", "Language" }
            : new[] { "SkillIds", "SkillsId", "SkillId", "Skills", "Skill" };

        var request = bindingContext.HttpContext.Request;
        bool hadMatchingKey = false;
        var result = new HashSet<int>();

        // ── 1. Check Request.Form ─────────────────────────────────────────────
        if (request.HasFormContentType && request.Form != null)
        {
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in request.Form.Keys)
            {
                var cleanKey = NormalizeKey(key);

                foreach (var root in rootNames)
                {
                    bool isMatch = string.Equals(cleanKey, root, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(cleanKey, $"{root}[]", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(cleanKey, $"{root}Json", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(cleanKey, $"{root}String", StringComparison.OrdinalIgnoreCase)
                                || cleanKey.StartsWith($"{root}[", StringComparison.OrdinalIgnoreCase);

                    if (isMatch)
                    {
                        if (processedKeys.Add(key))
                        {
                            hadMatchingKey = true;
                            foreach (var val in request.Form[key])
                            {
                                ParseIntValues(val, result);
                            }
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            // ── 2. Fallback to ValueProvider ──────────────────────────────────
            foreach (var root in rootNames)
            {
                var candidates = new[]
                {
                    root,
                    $"dto.{root}",
                    $"{root}[]",
                    $"{root}Json"
                };

                foreach (var key in candidates)
                {
                    var valResult = bindingContext.ValueProvider.GetValue(key);
                    if (valResult != ValueProviderResult.None)
                    {
                        hadMatchingKey = true;
                        foreach (var val in valResult)
                        {
                            ParseIntValues(val, result);
                        }
                    }
                }
            }
        }

        if (result.Count > 0)
        {
            bindingContext.Result = ModelBindingResult.Success(result.Where(x => x > 0).ToList());
        }
        else if (hadMatchingKey)
        {
            bindingContext.Result = ModelBindingResult.Success(new List<int>());
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }

        return Task.CompletedTask;
    }

    private static void ParseIntValues(string? val, HashSet<int> result)
    {
        if (string.IsNullOrWhiteSpace(val) || val.Equals("[object Object]", StringComparison.OrdinalIgnoreCase))
            return;

        val = val.Trim();

        if (val.StartsWith('[') && val.EndsWith(']'))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<int>>(val);
                if (list != null)
                {
                    foreach (var i in list) if (i > 0) result.Add(i);
                    return;
                }
            }
            catch { }

            try
            {
                using var doc = JsonDocument.Parse(val);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n) && n > 0)
                            result.Add(n);
                        else if (el.ValueKind == JsonValueKind.Object)
                        {
                            if (el.TryGetProperty("id", out var p1) && p1.TryGetInt32(out var n1) && n1 > 0)
                                result.Add(n1);
                            else if (el.TryGetProperty("skillId", out var p2) && p2.TryGetInt32(out var n2) && n2 > 0)
                                result.Add(n2);
                            else if (el.TryGetProperty("languageId", out var p3) && p3.TryGetInt32(out var n3) && n3 > 0)
                                result.Add(n3);
                        }
                    }
                    return;
                }
            }
            catch { }
        }

        var parts = val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var p in parts)
        {
            if (int.TryParse(p, out var parsed) && parsed > 0)
                result.Add(parsed);
        }
    }

    private static string NormalizeKey(string key)
    {
        var clean = key;
        if (clean.StartsWith("dto.", StringComparison.OrdinalIgnoreCase))
            clean = clean[4..];
        else if (clean.StartsWith("dto[", StringComparison.OrdinalIgnoreCase) && clean.IndexOf(']') > 4)
        {
            var close = clean.IndexOf(']');
            clean = clean[4..close].Trim('\'', '"') + clean[(close + 1)..];
        }
        return clean;
    }
}
