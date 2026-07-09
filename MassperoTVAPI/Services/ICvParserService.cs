using MassperoTVAPI.Core.DTOs;

namespace MassperoTVAPI.Services;

public interface ICvParserService
{
    /// <summary>
    /// Extracts structured data from a PDF CV file.
    /// </summary>
    /// <param name="pdfFilePath">Absolute physical path to the PDF file.</param>
    /// <param name="candidateId">The candidate's ID.</param>
    /// <param name="candidateName">The candidate's name.</param>
    /// <returns>Extracted CV data including skills, experience, languages, certificates, and education.</returns>
    CvExtractedDataDto Extract(string pdfFilePath, int candidateId, string candidateName);
}
