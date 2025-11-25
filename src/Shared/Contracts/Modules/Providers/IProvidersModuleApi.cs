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
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um provider pelo ID do usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do provider ou erro</returns>
    Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um provider pelo documento
    /// </summary>
    /// <param name="document">Documento do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do provider ou erro</returns>
    Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider existe
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o provider existe</returns>
    Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário é um provider
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o usuário é um provider</returns>
    Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um documento já está em uso
    /// </summary>
    /// <param name="document">Documento a ser verificado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o documento já está em uso</returns>
    Task<Result<bool>> DocumentExistsAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de providers por IDs (operação em lote)
    /// </summary>
    /// <param name="providerIds">Lista de IDs dos providers</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de informações básicas dos providers</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de providers por IDs
    /// </summary>
    /// <param name="providerIds">Lista de IDs dos providers</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de informações básicas dos providers</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por cidade
    /// </summary>
    /// <param name="city">Nome da cidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers da cidade especificada</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por estado
    /// </summary>
    /// <param name="state">Nome do estado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers do estado especificado</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por tipo
    /// </summary>
    /// <param name="providerType">Tipo do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers do tipo especificado</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(string providerType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por status de verificação
    /// </summary>
    /// <param name="verificationStatus">Status de verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de providers com o status especificado</returns>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(string verificationStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém dados completos do provider otimizados para indexação em busca.
    /// Retorna todos os dados necessários para criar/atualizar um SearchableProvider.
    /// </summary>
    /// <param name="providerId">ID do provider</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do provider para indexação, ou null se não encontrado</returns>
    Task<Result<ProviderIndexingDto?>> GetProviderForIndexingAsync(Guid providerId, CancellationToken cancellationToken = default);
}
