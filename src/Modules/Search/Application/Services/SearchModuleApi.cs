using MeAjudaAi.Modules.Search.Application.DTOs;
using MeAjudaAi.Modules.Search.Application.Queries;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Shared.Contracts.Modules.Search;
using MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Search.Application.Services;

/// <summary>
/// Implementation of the Search module's public API.
/// </summary>
public sealed class SearchModuleApi : ISearchModuleApi
{
    private readonly IQueryDispatcher _queryDispatcher;

    public SearchModuleApi(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    public async Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        SubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Map module DTOs to domain enums using explicit mapping
        ESubscriptionTier[]? domainTiers = subscriptionTiers?.Select(ToDomainTier).ToArray();

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            serviceIds,
            minRating,
            domainTiers,
            pageNumber,
            pageSize);

        var result = await _queryDispatcher.QueryAsync<SearchProvidersQuery, Result<PagedSearchResultDto<SearchableProviderDto>>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return Result<ModulePagedSearchResultDto>.Failure(result.Error);
        }

        // Map internal DTOs to module DTOs
        var moduleResult = new ModulePagedSearchResultDto
        {
            Items = result.Value!.Items.Select(p => new ModuleSearchableProviderDto
            {
                ProviderId = p.ProviderId,
                Name = p.Name,
                Description = p.Description,
                Location = new ModuleLocationDto
                {
                    Latitude = p.Location.Latitude,
                    Longitude = p.Location.Longitude
                },
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                SubscriptionTier = ToModuleTier(p.SubscriptionTier),
                ServiceIds = p.ServiceIds,
                DistanceInKm = p.DistanceInKm,
                City = p.City,
                State = p.State
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize
        };

        return Result<ModulePagedSearchResultDto>.Success(moduleResult);
    }

    /// <summary>
    /// Maps module tier enum to domain tier enum with explicit conversion.
    /// </summary>
    private static ESubscriptionTier ToDomainTier(SubscriptionTier tier) => tier switch
    {
        SubscriptionTier.Free => ESubscriptionTier.Free,
        SubscriptionTier.Standard => ESubscriptionTier.Standard,
        SubscriptionTier.Gold => ESubscriptionTier.Gold,
        SubscriptionTier.Platinum => ESubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };

    /// <summary>
    /// Maps domain tier enum to module tier enum with explicit conversion.
    /// </summary>
    private static SubscriptionTier ToModuleTier(ESubscriptionTier tier) => tier switch
    {
        ESubscriptionTier.Free => SubscriptionTier.Free,
        ESubscriptionTier.Standard => SubscriptionTier.Standard,
        ESubscriptionTier.Gold => SubscriptionTier.Gold,
        ESubscriptionTier.Platinum => SubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };
}
