using MeAjudaAi.Modules.Search.Domain.Entities;

namespace MeAjudaAi.Modules.Search.Domain.ValueObjects;

/// <summary>
/// Represents the result of a provider search operation.
/// </summary>
public sealed record SearchResult
{
    /// <summary>
    /// List of providers matching the search criteria.
    /// </summary>
    public required IReadOnlyList<SearchableProvider> Providers { get; init; }

    /// <summary>
    /// Total number of providers matching the search criteria (before pagination).
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Indicates if there are more results available.
    /// </summary>
    public bool HasMore => Providers.Count < TotalCount;
}
