using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Documents;

/// <summary>
/// Mock de IDocumentIntelligenceService para testes de integração e E2E.
/// Simula operações OCR sem Azure Document Intelligence real.
/// </summary>
public sealed class MockDocumentIntelligenceService : IDocumentIntelligenceService
{
    private bool _simulateLowConfidence;
    private bool _simulateError;
    private string? _errorMessage;

    public void SetNextResultToLowConfidence()
    {
        _simulateLowConfidence = true;
        _simulateError = false;
    }

    public void SetNextResultToError(string errorMessage = "OCR service unavailable")
    {
        _simulateError = true;
        _simulateLowConfidence = false;
        _errorMessage = errorMessage;
    }

    public void Reset()
    {
        _simulateLowConfidence = false;
        _simulateError = false;
        _errorMessage = null;
    }

    public Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
            throw new ArgumentException("Blob URL cannot be null or empty", nameof(blobUrl));

        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));

        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out _))
            throw new ArgumentException($"Invalid blob URL format: {blobUrl}", nameof(blobUrl));

        if (_simulateError)
        {
            var errorResult = new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: _errorMessage ?? "OCR service error"
            );
            Reset();
            return Task.FromResult(errorResult);
        }

        if (_simulateLowConfidence)
        {
            var lowConfidenceFields = new Dictionary<string, string>
            {
                ["documentNumber"] = "???",
                ["name"] = "Unreadable"
            };

            var lowConfidenceResult = new OcrResult(
                Success: true,
                ExtractedData: "{\"quality\":\"low\"}",
                Fields: lowConfidenceFields,
                Confidence: 0.45f,
                ErrorMessage: null
            );
            Reset();
            return Task.FromResult(lowConfidenceResult);
        }

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
