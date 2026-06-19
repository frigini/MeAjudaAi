using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Interface Refit para a API REST de Documents.
/// Define endpoints HTTP para gestão de documentos de providers (upload, verificação, histórico).
/// </summary>
/// <remarks>
/// Esta interface é usada pelo Refit para gerar automaticamente
/// o cliente HTTP tipado. Retorna Result&lt;T&gt; para tratamento
/// funcional de erros no frontend Blazor WASM.
/// </remarks>
public interface IDocumentsApi
{
    /// <summary>
    /// Gera URL de upload com SAS token para envio direto ao Azure Blob Storage.
    /// </summary>
    /// <param name="request">Dados do documento para upload (ProviderId, DocumentType, FileName, ContentType, FileSizeBytes)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>URL de upload com SAS token</returns>
    /// <response code="200">URL gerada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para upload de documentos deste provider</response>
    [Post("/api/v1/documents/upload")]
    Task<Result<UploadDocumentResponse>> UploadDocumentAsync(
        [Body] UploadDocumentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos os documentos de um provider.
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de documentos com status de verificação</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para visualizar documentos deste provider</response>
    /// <response code="404">Provider não encontrado</response>
    [Get("/api/v1/documents/provider/{providerId}")]
    Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetDocumentsByProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um documento específico por ID.
    /// </summary>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Detalhes completos do documento incluindo status e dados de OCR</returns>
    /// <response code="200">Documento encontrado</response>
    /// <response code="404">Documento não encontrado</response>
    /// <response code="401">Não autenticado</response>
    [Get("/api/v1/documents/{documentId}")]
    Task<Result<ModuleDocumentDto>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui um documento e seu blob associado (Admin only).
    /// </summary>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <response code="200">Documento excluído com sucesso</response>
    /// <response code="404">Documento não encontrado</response>
    /// <response code="403">Sem permissão de administrador</response>
    /// <response code="401">Não autenticado</response>
    [Delete("/api/v1/documents/{documentId}")]
    Task<Result<bool>> DeleteDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solicita verificação manual de um documento quando OCR falha ou precisa validação adicional.
    /// </summary>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <response code="202">Verificação solicitada com sucesso (processamento assíncrono)</response>
    /// <response code="404">Documento não encontrado</response>
    /// <response code="409">Documento já está em processo de verificação</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para solicitar verificação</response>
    [Post("/api/v1/documents/{documentId}/request-verification")]
    Task RequestDocumentVerificationAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aprova ou rejeita um documento após verificação manual (Admin only).
    /// </summary>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="request">Dados da verificação (IsVerified + VerificationNotes)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <response code="200">Documento verificado com sucesso</response>
    /// <response code="400">Documento já verificado ou status inválido</response>
    /// <response code="404">Documento não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Post("/api/v1/documents/{documentId}/verify")]
    Task<Result> VerifyDocumentAsync(
        Guid documentId,
        [Body] VerifyDocumentRequest request,
        CancellationToken cancellationToken = default);
}
