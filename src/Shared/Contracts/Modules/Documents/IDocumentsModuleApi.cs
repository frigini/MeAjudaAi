using MeAjudaAi.Shared.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Documents;

/// <summary>
/// Interface da API pública do módulo Documents para comunicação entre módulos.
/// </summary>
public interface IDocumentsModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém um documento por ID.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do documento ou null se não encontrado</returns>
    Task<Result<ModuleDocumentDto?>> GetDocumentByIdAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os documentos de um provider.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de documentos do provider</returns>
    Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetProviderDocumentsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém apenas o status de um documento.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status do documento</returns>
    Task<Result<ModuleDocumentStatusDto?>> GetDocumentStatusAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider tem documentos verificados.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possui documentos verificados</returns>
    Task<Result<bool>> HasVerifiedDocumentsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider completou o upload dos documentos obrigatórios.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se todos documentos obrigatórios foram enviados</returns>
    Task<Result<bool>> HasRequiredDocumentsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém contadores de documentos por status para um provider.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Contadores por status</returns>
    Task<Result<DocumentStatusCountDto>> GetDocumentStatusCountAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider tem documentos pendentes de verificação.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se há documentos pendentes</returns>
    Task<Result<bool>> HasPendingDocumentsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider tem algum documento rejeitado.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se há documentos rejeitados</returns>
    Task<Result<bool>> HasRejectedDocumentsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);
}
