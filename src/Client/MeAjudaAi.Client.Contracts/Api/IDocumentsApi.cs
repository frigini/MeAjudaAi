using MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Shared.Contracts.Functional;
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
    /// Faz upload de um documento de um provider.
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="file">Arquivo do documento (PDF, JPEG, PNG)</param>
    /// <param name="documentType">Tipo: "RG", "CNH", "CNPJ", "ComprovateResidencia", "Outros"</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>ID do documento criado e URL de acesso</returns>
    /// <response code="201">Documento enviado com sucesso</response>
    /// <response code="400">Arquivo inválido ou tipo de documento não suportado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para upload de documentos deste provider</response>
    /// <response code="413">Arquivo muito grande (máximo 10MB)</response>
    [Multipart]
    [Post("/api/v1/providers/{providerId}/documents")]
    Task<Result<ModuleDocumentDto>> UploadDocumentAsync(
        Guid providerId,
        [AliasAs("file")] StreamPart file,
        [AliasAs("documentType")] string documentType,
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
    [Get("/api/v1/providers/{providerId}/documents")]
    Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetDocumentsByProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um documento específico pelo ID.
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Detalhes completos do documento incluindo dados de OCR</returns>
    /// <response code="200">Documento encontrado</response>
    /// <response code="404">Documento ou provider não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para visualizar este documento</response>
    [Get("/api/v1/providers/{providerId}/documents/{documentId}")]
    Task<Result<ModuleDocumentDto>> GetDocumentByIdAsync(
        Guid providerId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solicita verificação de um documento via Azure Document Intelligence.
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Status da solicitação de verificação</returns>
    /// <response code="200">Verificação solicitada com sucesso (processamento assíncrono)</response>
    /// <response code="400">Documento em estado inválido para verificação</response>
    /// <response code="404">Documento ou provider não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão para solicitar verificação</response>
    [Post("/api/v1/providers/{providerId}/documents/{documentId}/verify")]
    Task<Result<Unit>> RequestDocumentVerificationAsync(
        Guid providerId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status de verificação de um documento (Admin only).
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="status">Novo status: "Pending", "Verified", "Rejected"</param>
    /// <param name="rejectionReason">Motivo da rejeição (obrigatório se status = Rejected)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Confirmação da atualização</returns>
    /// <response code="200">Status atualizado com sucesso</response>
    /// <response code="400">Transição de status inválida ou motivo de rejeição ausente</response>
    /// <response code="404">Documento ou provider não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Put("/api/v1/providers/{providerId}/documents/{documentId}/status")]
    Task<Result<Unit>> UpdateDocumentStatusAsync(
        Guid providerId,
        Guid documentId,
        [Query] string status,
        [Query] string? rejectionReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um documento (Admin only).
    /// </summary>
    /// <param name="providerId">ID do provider (GUID)</param>
    /// <param name="documentId">ID do documento (GUID)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="204">Documento deletado com sucesso</response>
    /// <response code="404">Documento ou provider não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Sem permissão de administrador</response>
    [Delete("/api/v1/providers/{providerId}/documents/{documentId}")]
    Task<Result<Unit>> DeleteDocumentAsync(
        Guid providerId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
