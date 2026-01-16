using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Application.Handlers;

/// <summary>
/// Handler para buscar prestadores com base em localização e critérios.
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

        // Cria localização usando GeoPoint (lança exceção em coordenadas inválidas)
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

        // Calcula paginação de forma defensiva
        var skip = Math.Max(0, (query.Page - 1) * query.PageSize);

        // Executa busca
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

        // Mapeia para DTOs usando distâncias pré-computadas do repositório
        // Distância é calculada uma vez no repositório (filter/sort/cache) para evitar cálculos redundantes
        var providerDtos = searchResult.Providers
            .Select((p, index) => new SearchableProviderDto
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
                DistanceInKm = searchResult.DistancesInKm[index],
                City = p.City,
                State = p.State
            })
            .ToList();

        var result = PagedResult<SearchableProviderDto>.Create(
            providerDtos,
            query.Page,
            query.PageSize,
            searchResult.TotalCount);

        return Result<PagedResult<SearchableProviderDto>>.Success(result);
    }
}
