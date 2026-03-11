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
        _logger.LogWarning(
            "DocumentIntelligenceService not configured. OCR analysis for document type '{DocumentType}' is unavailable.",
            documentType);

        return Task.FromResult(new OcrResult(
            Success: false,
            ExtractedData: null,
            Fields: null,
            Confidence: null,
            ErrorMessage: "Azure AI Document Intelligence is not configured. "
                + "Set 'Azure:DocumentIntelligence:Endpoint' and 'Azure:DocumentIntelligence:ApiKey' to enable OCR."));
    }
}
