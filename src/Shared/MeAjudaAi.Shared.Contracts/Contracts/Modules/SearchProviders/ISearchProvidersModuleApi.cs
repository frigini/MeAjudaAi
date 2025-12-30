using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Shared.Contracts.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders;

/// <summary>
/// API pública para o módulo de Busca e Descoberta.
/// </summary>
public interface ISearchProvidersModuleApi : IModuleApi
{
    /// <summary>
    /// Busca prestadores baseado em geolocalização e outros critérios.
    /// </summary>
    /// <param name="latitude">Latitude do ponto central da busca</param>
    /// <param name="longitude">Longitude do ponto central da busca</param>
    /// <param name="radiusInKm">Raio de busca em quilômetros</param>
    /// <param name="serviceIds">Filtro opcional por IDs de serviços</param>
    /// <param name="minRating">Filtro opcional de avaliação mínima</param>
    /// <param name="subscriptionTiers">Filtro opcional por níveis de assinatura</param>
    /// <param name="pageNumber">Número da página para paginação (base 1)</param>
    /// <param name="pageSize">Número de resultados por página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de prestadores pesquisáveis</returns>
    Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexa ou atualiza um prestador no índice de busca.
    /// Chamado quando o prestador é verificado/ativado para torná-lo descobrível.
    /// </summary>
    /// <param name="providerId">ID do prestador a indexar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado indicando sucesso ou falha</returns>
    Task<Result> IndexProviderAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um prestador do índice de busca.
    /// Chamado quando o prestador é rejeitado, suspenso ou deletado.
    /// </summary>
    /// <param name="providerId">ID do prestador a remover</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado indicando sucesso ou falha</returns>
    Task<Result> RemoveProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
}

