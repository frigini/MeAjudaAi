using MeAjudaAi.Modules.Search.Domain.Entities;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.Search.Domain.Repositories;

/// <summary>
/// Repository for SearchableProvider aggregate.
/// </summary>
public interface ISearchableProviderRepository
{
    /// <summary>
    /// Retrieves a searchable provider by its ID.
    /// </summary>
    Task<SearchableProvider?> GetByIdAsync(SearchableProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a searchable provider by the original provider ID.
    /// </summary>
    Task<SearchableProvider?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for providers within a specified radius of a location.
    /// Results are ordered by subscription tier (desc), average rating (desc), and distance (asc).
    /// </summary>
    /// <param name="location">Center point for the search</param>
    /// <param name="radiusInKm">Maximum distance in kilometers</param>
    /// <param name="serviceIds">Optional list of service IDs to filter by</param>
    /// <param name="minRating">Optional minimum average rating filter</param>
    /// <param name="subscriptionTiers">Optional list of subscription tiers to filter by</param>
    /// <param name="skip">Number of results to skip for pagination (must be non-negative)</param>
    /// <param name="take">Number of results to return (must be positive and within configured max page size)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search result containing providers and total count</returns>
    Task<SearchResult> SearchAsync(
        GeoPoint location,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new searchable provider to the repository.
    /// </summary>
    Task AddAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing searchable provider.
    /// </summary>
    Task UpdateAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a searchable provider from the repository.
    /// </summary>
    Task DeleteAsync(SearchableProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
