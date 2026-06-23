using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.SearchProviders.Application.Queries;

/// <summary>
/// Interface para consultas do módulo SearchProviders.
/// </summary>
public interface ISearchableProviderQueries
{
    /// <summary>
    /// Verifica se o módulo consegue conectar ao banco de dados.
    /// </summary>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um provedor pesquisável por seu ID.
    /// </summary>
    Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um provedor pesquisável pelo ID do provedor original.
    /// </summary>
    /// <param name="providerId">ID do provedor original</param>
    /// <param name="track">Se verdadeiro, a entidade retornada será rastreada pelo EF Core (para modificações)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, bool track = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca provedores dentro de um raio especificado de uma localização.
    /// Os resultados são ordenados por tier de assinatura (desc), avaliação média (desc) e distância (asc).
    /// </summary>
    Task<SearchResult> SearchAsync(
        GeoPoint location,
        double radiusInKm,
        string? term = null,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os provedores que oferecem um serviço específico.
    /// </summary>
    /// <param name="serviceId">ID do serviço</param>
    /// <param name="track">Se verdadeiro, as entidades retornadas serão rastreadas pelo EF Core (para modificações)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<IReadOnlyList<SearchableProvider>> GetByServiceIdAsync(Guid serviceId, bool track = false, CancellationToken cancellationToken = default);
}
