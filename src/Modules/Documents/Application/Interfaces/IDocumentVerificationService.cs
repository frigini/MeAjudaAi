namespace MeAjudaAi.Modules.Documents.Application.Interfaces;

/// <summary>
/// Serviço para processar verificação de documentos enviados.
/// Executa OCR, validações e verificações de antecedentes.
/// </summary>
public interface IDocumentVerificationService
{
    /// <summary>
    /// Processa um documento específico, executando OCR e verificações
    /// </summary>
    /// <param name="documentId">ID do documento a ser processado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
