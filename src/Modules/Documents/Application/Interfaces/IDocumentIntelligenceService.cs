namespace MeAjudaAi.Modules.Documents.Application.Interfaces;

/// <summary>
/// Resultado do processamento OCR de um documento
/// </summary>
/// <remarks>
/// <para>
/// Para acesso type-safe aos campos extraídos, use as constantes em
/// <see cref="Constants.DocumentModelConstants.OcrFieldKeys"/>.
/// Exemplo: result.Fields?[DocumentModelConstants.OcrFieldKeys.Cpf]
/// </para>
/// <para>
/// <strong>Invariante:</strong> Quando Success == false, ErrorMessage deve ser não-nulo
/// para fornecer contexto sobre a falha.
/// </para>
/// </remarks>
public record OcrResult(
    bool Success,
    string? ExtractedData,
    IReadOnlyDictionary<string, string>? Fields,
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
    /// <param name="documentType">Tipo de documento esperado. Use constantes de <see cref="Constants.DocumentModelConstants.DocumentTypes"/></param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da análise OCR</returns>
    Task<OcrResult> AnalyzeDocumentAsync(
        string blobUrl,
        string documentType,
        CancellationToken cancellationToken = default);
}
