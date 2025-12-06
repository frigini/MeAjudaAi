using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Mocks;

/// <summary>
/// Mock implementation of <see cref="IDocumentIntelligenceService"/> for integration testing.
/// Returns predefined OCR results without making actual calls to external services.
/// </summary>
/// <seealso cref="IDocumentIntelligenceService"/>
public class MockDocumentIntelligenceService : IDocumentIntelligenceService
{
    /// <summary>
    /// Simulates document analysis by returning a successful OCR result with mock data.
    /// </summary>
    /// <param name="blobUrl">The URL of the blob containing the document to analyze.</param>
    /// <param name="documentType">The type of document being analyzed (e.g., identity document, proof of residence).</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A Task containing a mock <see cref="OcrResult"/> with predefined field values and high confidence.</returns>
    public Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        // Mock successful OCR analysis
        var fields = new Dictionary<string, string>
        {
            ["documentNumber"] = "12345678900",
            ["name"] = "Test User",
            ["dateOfBirth"] = "1990-01-01",
            ["issueDate"] = "2020-01-01",
            ["expiryDate"] = "2030-01-01"
        };

        var result = new OcrResult(
            Success: true,
            ExtractedData: "Mock extracted text data from document",
            Fields: fields,
            Confidence: 0.95f,
            ErrorMessage: null
        );

        return Task.FromResult(result);
    }
}
