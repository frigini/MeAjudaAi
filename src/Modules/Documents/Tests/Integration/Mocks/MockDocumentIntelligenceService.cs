using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Mocks;

public class MockDocumentIntelligenceService : IDocumentIntelligenceService
{
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
