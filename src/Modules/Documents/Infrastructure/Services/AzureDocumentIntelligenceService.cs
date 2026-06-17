using Azure;
using Azure.AI.DocumentIntelligence;
using MeAjudaAi.Modules.Documents.Application.Constants;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

internal class AzureDocumentIntelligenceService(
    DocumentIntelligenceClient client,
    ILogger<AzureDocumentIntelligenceService> logger,
    [FromKeyedServices(SerializationKeys.Logging)] ISerializer serializer) : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<AzureDocumentIntelligenceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

    public async Task<OcrResult> AnalyzeDocumentAsync(
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

        // Validar formato da URL para falhar rápido antes de chamar a API
        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var validatedUri))
        {
            throw new ArgumentException($"Invalid blob URL format: {blobUrl}", nameof(blobUrl));
        }

        try
        {
            _logger.LogInformation("Starting OCR analysis for document type {DocumentType}", documentType);

            // Usar constantes centralizadas de IDs de modelo para evitar strings mágicas
            string modelId = documentType.ToLowerInvariant() switch
            {
                DocumentTypes.IdentityDocument => ModelIds.IdentityDocument,
                DocumentTypes.ProofOfResidence => ModelIds.GenericDocument,
                DocumentTypes.CriminalRecord => ModelIds.GenericDocument,
                _ => ModelIds.GenericDocument
            };

            // Usar AnalyzeDocumentAsync da nova API Azure.AI.DocumentIntelligence
            // A API mudou: agora usa BinaryData ou Uri diretamente
            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                modelId,
                validatedUri,
                cancellationToken: cancellationToken);

            var result = operation.Value;

            // Extrai campos dos documentos analisados
            var fields = new Dictionary<string, string>();
            float totalConfidence = 0;
            int fieldCount = 0;

            foreach (var document in result.Documents)
            {
                foreach (var field in document.Fields)
                {
                    if (field.Value.Content != null)
                    {
                        fields[field.Key] = field.Value.Content;
                        if (field.Value.Confidence.HasValue)
                        {
                            totalConfidence += field.Value.Confidence.Value;
                            fieldCount++;
                        }
                    }
                }
            }

            var averageConfidence = fieldCount > 0 ? totalConfidence / fieldCount : 0;

            var extractedData = _serializer.Serialize(fields);

            _logger.LogInformation(
                "OCR analysis completed successfully. Average confidence: {Confidence:P}, Fields extracted: {FieldCount}",
                averageConfidence, fieldCount);

            return new OcrResult(
                Success: true,
                ExtractedData: extractedData,
                Fields: fields,
                Confidence: averageConfidence,
                ErrorMessage: null);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error processing document with Azure Document Intelligence");
            return new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: $"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing document");
            return new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: ex.Message);
        }
    }
}
