using MeAjudaAi.Modules.SearchProviders.Domain.Entities;

namespace MeAjudaAi.Modules.SearchProviders.Domain.Models;

/// <summary>
/// Representa o resultado de uma operação de busca de provedores.
/// Este é um container de resultados de query, não um Value Object no sentido DDD,
/// pois contém entidades mutáveis (SearchableProvider).
/// </summary>
public sealed record SearchResult(
    IReadOnlyList<SearchableProvider> Providers,
    IReadOnlyList<double> DistancesInKm,
    int TotalCount)
{
    /// <summary>
    /// Indica se há mais resultados disponíveis além da página atual.
    /// Retorna true quando TotalCount é maior que o número de provedores retornados na página atual,
    /// sinalizando que páginas adicionais podem ser buscadas.
    /// </summary>
    public bool HasMore => Providers.Count < TotalCount;
}
