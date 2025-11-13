namespace MeAjudaAi.Modules.Documents.Application.Interfaces;

/// <summary>
/// Resultado do processamento OCR de um documento
/// </summary>
public record OcrResult(
    bool Success,
    string? ExtractedData,
    Dictionary<string, string>? Fields,
    float? Confidence,
    string? ErrorMessage);

/// <summary>
/// Serviço para análise de documentos usando Azure AI Document Intelligence
/// </summary>
public interface IDocumentIntelligenceService
{
    /// <summary>
    /// Analisa um documento brasileiro (RG, CPF, CNH) e extrai informações
    /// </summary>
    /// <param name="blobUrl">URL do blob contendo o documento</param>
    /// <param name="documentType">Tipo de documento esperado (IdentityDocument, etc)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da análise OCR</returns>
    Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default);
}
