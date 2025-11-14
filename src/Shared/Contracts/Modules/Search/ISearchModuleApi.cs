using MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Search;

/// <summary>
/// Public API for the Search & Discovery module.
/// </summary>
public interface ISearchModuleApi
{
    /// <summary>
    /// Searches for providers based on geolocation and other criteria.
    /// </summary>
    /// <param name="latitude">Latitude of search center point</param>
    /// <param name="longitude">Longitude of search center point</param>
    /// <param name="radiusInKm">Search radius in kilometers</param>
    /// <param name="serviceIds">Optional filter by service IDs</param>
    /// <param name="minRating">Optional minimum rating filter</param>
    /// <param name="subscriptionTiers">Optional filter by subscription tiers</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of searchable providers</returns>
    Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        SubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
