using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Mock implementation of IDocumentIntelligenceService for testing environments
/// </summary>
public sealed class MockDocumentIntelligenceService : IDocumentIntelligenceService
{
    public Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl, 
        string documentType, 
        CancellationToken cancellationToken = default)
    {
        // Retorna dados OCR mockados para testes
        var mockFields = new Dictionary<string, string>
        {
            ["documentNumber"] = "123456789",
            ["name"] = "Test User",
            ["issueDate"] = "2024-01-01",
            ["expiryDate"] = "2034-01-01"
        };

        var mockExtractedData = $$"""
        {
            "documentType": "{{documentType}}",
            "documentNumber": "123456789",
            "name": "Test User",
            "issueDate": "2024-01-01",
            "expiryDate": "2034-01-01"
        }
        """;

        var result = new OcrResult(
            Success: true,
            ExtractedData: mockExtractedData,
            Fields: mockFields,
            Confidence: 0.95f,
            ErrorMessage: null
        );

        return Task.FromResult(result);
    }
}
