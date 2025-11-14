using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Mock implementation of IDocumentIntelligenceService for testing environments
/// </summary>
/// <remarks>
/// Por padrão, retorna resultados de sucesso com alta confiança.
/// Para testar cenários de falha, use os métodos configuráveis:
/// - SetNextResultToLowConfidence() para simular baixa qualidade de OCR
/// - SetNextResultToError() para simular erro na API
/// </remarks>
public sealed class MockDocumentIntelligenceService : IDocumentIntelligenceService
{
    private bool _simulateLowConfidence;
    private bool _simulateError;
    private string? _errorMessage;

    /// <summary>
    /// Configura o próximo resultado para retornar baixa confiança (útil para testar fluxo de rejeição)
    /// </summary>
    public void SetNextResultToLowConfidence()
    {
        _simulateLowConfidence = true;
        _simulateError = false;
    }

    /// <summary>
    /// Configura o próximo resultado para retornar erro (útil para testar fluxo de falha)
    /// </summary>
    public void SetNextResultToError(string errorMessage = "OCR service unavailable")
    {
        _simulateError = true;
        _simulateLowConfidence = false;
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// Reseta para o comportamento padrão (sucesso com alta confiança)
    /// </summary>
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
        // Simular erro de API se configurado
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

        // Simular baixa confiança se configurado
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
                Confidence: 0.45f, // Abaixo do threshold típico de 0.7
                ErrorMessage: null
            );
            Reset();
            return Task.FromResult(lowConfidenceResult);
        }

        // Retorna dados OCR mockados para happy path
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
