using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Providers;

/// <summary>
/// Interface da API pública do módulo Providers para outros módulos
/// </summary>
public interface IProvidersModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém um provider pelo ID
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do provider ou erro</returns>
    Task<Result<ModuleProviderDto>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um provider pelo documento
    /// </summary>
    /// <param name="document">Documento do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do provider ou erro</returns>
    Task<Result<ModuleProviderDto>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de providers por IDs
    /// </summary>
    /// <param name="providerIds">Lista de IDs dos providers</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de informações básicas dos providers</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por tipo
    /// </summary>
    /// <param name="providerType">Tipo do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers do tipo especificado</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(EProviderType providerType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por status de verificação
    /// </summary>
    /// <param name="verificationStatus">Status de verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers com o status especificado</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(EVerificationStatus verificationStatus, CancellationToken cancellationToken = default);
}