using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

public class AzureDocumentIntelligenceService(DocumentAnalysisClient client, ILogger<AzureDocumentIntelligenceService> logger) : IDocumentIntelligenceService
{
    private readonly DocumentAnalysisClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<AzureDocumentIntelligenceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando análise OCR para documento tipo {DocumentType}", documentType);

            // Azure Document Intelligence usa modelos pré-construídos
            // Para documentos brasileiros, usamos o modelo "prebuilt-idDocument"
            string modelId = documentType.ToLowerInvariant() switch
            {
                "identitydocument" => "prebuilt-idDocument",
                "prooofresidence" => "prebuilt-document", // Modelo genérico para comprovantes
                _ => "prebuilt-document"
            };

            var operation = await _client.AnalyzeDocumentFromUriAsync(
                WaitUntil.Completed,
                modelId,
                new Uri(blobUrl),
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
