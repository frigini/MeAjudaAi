using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Implementação nula (no-op) de <see cref="IDocumentIntelligenceService"/> usada quando
/// o Azure AI Document Intelligence não está configurado (ex.: ambiente de desenvolvimento local,
/// geração de spec OpenAPI via Swagger CLI).
/// Todos os métodos retornam um <see cref="OcrResult"/> de falha com mensagem explicativa.
/// </summary>
internal sealed class NullDocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly ILogger<NullDocumentIntelligenceService> _logger;

    public NullDocumentIntelligenceService(ILogger<NullDocumentIntelligenceService> logger)
    {
        _logger = logger;
    }

    public Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
        {
            throw new ArgumentException("Blob URL cannot be null or empty", nameof(blobUrl));
        }

        if (string.IsNullOrWhiteSpace(documentType))
        {
            throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
        }

        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"Invalid blob URL format: {blobUrl}", nameof(blobUrl));
        }

        _logger.LogWarning(
            "DocumentIntelligenceService not configured. OCR analysis for document type '{DocumentType}' is unavailable.",
            documentType);

        return Task.FromResult(new OcrResult(
            Success: false,
            ExtractedData: null,
            Fields: null,
            Confidence: null,
            // English message is intended for logs/operational use by developers/operators.
            ErrorMessage: "Azure AI Document Intelligence is not configured. "
                + "Set 'Azure:DocumentIntelligence:Endpoint' and 'Azure:DocumentIntelligence:ApiKey' to enable OCR."));
    }
}
