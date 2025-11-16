using MeAjudaAi.Modules.Search.Application.DTOs;
using MeAjudaAi.Modules.Search.Application.Queries;
using MeAjudaAi.Modules.Search.Domain.Repositories;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Search.Application.Handlers;

/// <summary>
/// Handler for searching providers based on location and criteria.
/// </summary>
public sealed class SearchProvidersQueryHandler(
    ISearchableProviderRepository repository,
    ILogger<SearchProvidersQueryHandler> logger)
    : IQueryHandler<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>
{
    public async Task<Result<PagedResult<SearchableProviderDto>>> HandleAsync(
        SearchProvidersQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            "Searching providers at ({Latitude}, {Longitude}) within {Radius}km",
            query.Latitude,
            query.Longitude,
            query.RadiusInKm);

        // Create location using GeoPoint (throws on invalid coordinates)
        GeoPoint location;
        try
        {
            location = new GeoPoint(query.Latitude, query.Longitude);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogWarning(
                ex,
                "Invalid coordinates received for provider search");

            return Result<PagedResult<SearchableProviderDto>>.Failure(ex.Message);
        }

        // Calculate pagination defensively
        var skip = Math.Max(0, (query.Page - 1) * query.PageSize);

        // Execute search
        var searchResult = await repository.SearchAsync(
            location,
            query.RadiusInKm,
            query.ServiceIds,
            query.MinRating,
            query.SubscriptionTiers,
            skip,
            query.PageSize,
            cancellationToken);

        logger.LogInformation(
            "Found {Count} providers out of {Total} total matches",
            searchResult.Providers.Count,
            searchResult.TotalCount);

        // Map to DTOs
        var providerDtos = searchResult.Providers.Select(p => new SearchableProviderDto
        {
            ProviderId = p.ProviderId,
            Name = p.Name,
            Description = p.Description,
            Location = new LocationDto
            {
                Latitude = p.Location.Latitude,
                Longitude = p.Location.Longitude
            },
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            SubscriptionTier = p.SubscriptionTier,
            ServiceIds = p.ServiceIds,
            DistanceInKm = p.CalculateDistanceToInKm(location),
            City = p.City,
            State = p.State
        }).ToList();

        var result = PagedResult<SearchableProviderDto>.Create(
            providerDtos,
            query.Page,
            query.PageSize,
            searchResult.TotalCount);

        return Result<PagedResult<SearchableProviderDto>>.Success(result);
    }
}
