using System.Text.Json;
using Azure;
using Azure.AI.DocumentIntelligence;
using MeAjudaAi.Modules.Documents.Application.Constants;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

public class AzureDocumentIntelligenceService(DocumentIntelligenceClient client, ILogger<AzureDocumentIntelligenceService> logger) : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<AzureDocumentIntelligenceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentType))
        {
            throw new ArgumentNullException(nameof(documentType), "Document type cannot be null or empty");
        }

        try
        {
            _logger.LogInformation("Iniciando análise OCR para documento tipo {DocumentType}", documentType);

            // Use centralized constants for model IDs to avoid magic strings
            string modelId = documentType.ToLowerInvariant() switch
            {
                DocumentModelConstants.DocumentTypes.IdentityDocument => DocumentModelConstants.ModelIds.IdentityDocument,
                DocumentModelConstants.DocumentTypes.ProofOfResidence => DocumentModelConstants.ModelIds.GenericDocument,
                DocumentModelConstants.DocumentTypes.CriminalRecord => DocumentModelConstants.ModelIds.GenericDocument,
                _ => DocumentModelConstants.ModelIds.GenericDocument
            };

            // Usar AnalyzeDocumentAsync da nova API Azure.AI.DocumentIntelligence
            // A API mudou: agora usa BinaryData ou Uri diretamente
            var uriSource = new Uri(blobUrl);
            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                modelId,
                uriSource,
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

            var extractedData = JsonSerializer.Serialize(fields, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation(
                "Análise OCR concluída com sucesso. Confiança média: {Confidence:P}, Campos extraídos: {FieldCount}",
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
            _logger.LogError(ex, "Erro ao processar documento com Azure Document Intelligence");
            return new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: $"Erro na API: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar documento");
            return new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: ex.Message);
        }
    }
}
