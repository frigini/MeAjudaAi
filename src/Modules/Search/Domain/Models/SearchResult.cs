using MeAjudaAi.Modules.Search.Domain.Entities;

namespace MeAjudaAi.Modules.Search.Domain.Models;

/// <summary>
/// Representa o resultado de uma operação de busca de provedores.
/// Este é um container de resultados de query, não um Value Object no sentido DDD,
/// pois contém entidades mutáveis (SearchableProvider).
/// </summary>
public sealed record SearchResult
{
    /// <summary>
    /// Lista de provedores que correspondem aos critérios de busca.
    /// </summary>
    public required IReadOnlyList<SearchableProvider> Providers { get; init; }

    /// <summary>
    /// Distâncias pré-calculadas (em km) do ponto de busca para cada provedor.
    /// O índice corresponde à posição do provedor na lista Providers.
    /// Calculado uma vez no repositório para evitar recálculos redundantes no handler.
    /// </summary>
    public required IReadOnlyList<double> DistancesInKm { get; init; }

    /// <summary>
    /// Número total de provedores que correspondem aos critérios de busca (antes da paginação).
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Indica se há mais resultados disponíveis além da página atual.
    /// Retorna true quando TotalCount é maior que o número de provedores retornados na página atual,
    /// sinalizando que páginas adicionais podem ser buscadas.
    /// </summary>
    public bool HasMore => Providers.Count < TotalCount;
}
