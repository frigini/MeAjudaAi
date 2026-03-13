using System.Runtime.CompilerServices;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("MeAjudaAi.Modules.Documents.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

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

        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid blob URL format: {blobUrl}", nameof(blobUrl));
        }

        if (uri.Query.Contains("sig=") || uri.Query.Contains("SharedAccessSignature"))
        {
            // Se parecer conter um SAS token, sanitiza para a exceção
            var sanitizedUrl = uri.GetLeftPart(UriPartial.Path);
            throw new ArgumentException($"Invalid blob URL format (sanitized): {sanitizedUrl}", nameof(blobUrl));
        }

        _logger.LogWarning(
            "DocumentIntelligenceService not configured. OCR analysis for document type '{DocumentType}' is unavailable.",
            documentType);

        return Task.FromResult(new OcrResult(
            Success: false,
            ExtractedData: null,
            Fields: null,
            Confidence: null,
            ErrorMessage: "Não foi possível processar o documento no momento, tente novamente mais tarde."));
    }
}
