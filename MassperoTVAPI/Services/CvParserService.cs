using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using MassperoTVAPI.Core.DTOs;

namespace MassperoTVAPI.Services;

public class CvParserService : ICvParserService
{
    // ── Section heading patterns (case-insensitive) ──────────────────────────
    private static readonly Dictionary<string, string[]> SectionKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Skills"] = [
            "skills", "technical skills", "core skills", "key skills",
            "competencies", "core competencies", "technologies",
            "tools", "tools & technologies", "programming languages"
        ],
        ["Experience"] = [
            "experience", "work experience", "professional experience",
            "employment history", "work history", "career history",
            "professional background"
        ],
        ["Languages"] = [
            "languages", "language skills", "language proficiency"
        ],
        ["Certificates"] = [
            "certificates", "certifications", "certification",
            "professional certifications", "licenses", "licenses & certifications",
            "awards & certifications", "awards"
        ],
        ["Education"] = [
            "education", "educational background", "academic background",
            "academic qualifications", "qualifications", "degrees"
        ]
    };

    /// <summary>
    /// Builds a regex that matches any known section heading at the start of a line.
    /// </summary>
    private static readonly Regex SectionHeadingRegex = BuildSectionRegex();

    /// <inheritdoc/>
    public CvExtractedDataDto Extract(string pdfFilePath, int candidateId, string candidateName)
    {
        var fullText = ExtractTextFromPdf(pdfFilePath);
        var sections = ParseSections(fullText);

        return new CvExtractedDataDto(
            CandidateId:   candidateId,
            CandidateName: candidateName,
            Skills:        sections.GetValueOrDefault("Skills",       []),
            Experience:    sections.GetValueOrDefault("Experience",   []),
            Languages:     sections.GetValueOrDefault("Languages",    []),
            Certificates:  sections.GetValueOrDefault("Certificates", []),
            Education:     sections.GetValueOrDefault("Education",    [])
        );
    }

    // ── PDF text extraction ──────────────────────────────────────────────────

    private static string ExtractTextFromPdf(string pdfFilePath)
    {
        var sb = new StringBuilder();

        using var reader = new PdfReader(pdfFilePath);
        using var doc    = new PdfDocument(reader);

        for (var i = 1; i <= doc.GetNumberOfPages(); i++)
        {
            var page = doc.GetPage(i);
            var text = PdfTextExtractor.GetTextFromPage(page);
            sb.AppendLine(text);
        }

        return sb.ToString();
    }

    // ── Section parsing ──────────────────────────────────────────────────────

    private static Dictionary<string, List<string>> ParseSections(string text)
    {
        var result = new Dictionary<string, List<string>>();
        var lines  = text.Split('\n', StringSplitOptions.TrimEntries);

        string? currentSection = null;
        var     currentLines   = new List<string>();

        foreach (var line in lines)
        {
            var matchedSection = MatchSectionHeading(line);

            if (matchedSection is not null)
            {
                // Save previous section
                if (currentSection is not null)
                    result[currentSection] = CleanLines(currentLines);

                currentSection = matchedSection;
                currentLines   = [];
            }
            else if (currentSection is not null)
            {
                currentLines.Add(line);
            }
        }

        // Save last section
        if (currentSection is not null)
            result[currentSection] = CleanLines(currentLines);

        return result;
    }

    /// <summary>
    /// Checks if a line is a section heading and returns the canonical section name.
    /// </summary>
    private static string? MatchSectionHeading(string line)
    {
        // Remove common decorators
        var cleaned = line.Trim().TrimEnd(':', '-', '–', '—').Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        foreach (var (sectionName, keywords) in SectionKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (string.Equals(cleaned, keyword, StringComparison.OrdinalIgnoreCase))
                    return sectionName;
            }
        }

        // Also try regex match for lines like "SKILLS:" or "== Experience =="
        var match = SectionHeadingRegex.Match(cleaned);
        if (match.Success)
        {
            var matchedText = match.Groups["heading"].Value.Trim();
            foreach (var (sectionName, keywords) in SectionKeywords)
            {
                foreach (var keyword in keywords)
                {
                    if (string.Equals(matchedText, keyword, StringComparison.OrdinalIgnoreCase))
                        return sectionName;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Cleans raw lines: removes empty lines, strips bullet markers.
    /// </summary>
    private static List<string> CleanLines(List<string> lines)
    {
        var result = new List<string>();

        foreach (var rawLine in lines)
        {
            // Strip common bullet characters
            var line = rawLine
                .TrimStart('•', '●', '▪', '▸', '►', '-', '–', '—', '*', '·', '○', '◦')
                .TrimStart()
                .Trim();

            // Remove numbered list prefixes like "1." or "1)"
            line = Regex.Replace(line, @"^\d+[\.\)]\s*", "");

            if (!string.IsNullOrWhiteSpace(line))
                result.Add(line);
        }

        return result;
    }

    /// <summary>
    /// Builds a compiled regex that matches known section headings
    /// with optional leading/trailing decorators.
    /// </summary>
    private static Regex BuildSectionRegex()
    {
        var allKeywords = SectionKeywords.Values
            .SelectMany(k => k)
            .Select(Regex.Escape);

        var pattern = $@"^[\s=\-_#*]*(?<heading>{string.Join("|", allKeywords)})[\s=\-_#:*]*$";
        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
