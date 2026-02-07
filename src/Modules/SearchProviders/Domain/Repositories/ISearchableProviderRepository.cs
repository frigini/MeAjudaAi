using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Models;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.SearchProviders.Domain.Repositories;

/// <summary>
/// Repositório para o agregado SearchableProvider.
/// </summary>
public interface ISearchableProviderRepository
{
    /// <summary>
    /// Recupera um provedor pesquisável por seu ID.
    /// </summary>
    Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um provedor pesquisável pelo ID do provedor original.
    /// </summary>
    Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca provedores dentro de um raio especificado de uma localização.
    /// Os resultados são ordenados por tier de assinatura (desc), avaliação média (desc) e distância (asc).
    /// </summary>
    /// <param name="location">Ponto central para a busca</param>
    /// <param name="radiusInKm">Distância máxima em quilômetros</param>
    /// <param name="serviceIds">Lista opcional de IDs de serviços para filtrar</param>
    /// <param name="minRating">Filtro opcional de avaliação média mínima</param>
    /// <param name="subscriptionTiers">Lista opcional de tiers de assinatura para filtrar</param>
    /// <param name="skip">Número de resultados a pular para paginação (deve ser não-negativo)</param>
    /// <param name="take">Número de resultados a retornar (deve ser positivo e dentro do tamanho máximo de página configurado)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da busca contendo provedores e contagem total</returns>
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
    /// Adiciona um novo provedor pesquisável ao repositório.
    /// </summary>
    Task AddAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um provedor pesquisável existente.
    /// </summary>
    Task UpdateAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um provedor pesquisável do repositório.
    /// </summary>
    Task DeleteAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva todas as alterações pendentes no banco de dados.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
