using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.SearchProviders.API.Endpoints;

/// <summary>
/// Endpoint para buscar provedores de serviço por localização e critérios.
/// </summary>
public class SearchProvidersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de busca de provedores.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = CreateVersionedGroup(app, "search", "Search");

        group.MapGet("/providers", SearchProvidersAsync)
            .WithName("SearchProviders")
            .WithSummary("Search for service providers")
            .WithDescription("""
                Searches for active service providers based on geolocation and filters.
                
                **Search Algorithm:**
                1. Filter by radius from search location
                2. Apply optional filters (services, rating, subscription tier)
                3. Rank results by:
                   - Subscription tier (Platinum > Gold > Standard > Free)
                   - Average rating (highest first)
                   - Distance (closest first)
                
                **Use Cases:**
                - Find providers near a specific location
                - Search for providers offering specific services
                - Filter by minimum rating or subscription level
                """)
            .Produces<PagedResult<SearchableProviderDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SearchProvidersAsync(
        IQueryDispatcher queryDispatcher,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusInKm,
        [FromQuery] Guid[]? serviceIds,
        [FromQuery] decimal? minRating,
        [FromQuery] ESubscriptionTier[]? subscriptionTiers,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validar inputs no limite do endpoint
        if (page < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "page must be greater than or equal to 1",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (pageSize <= 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "pageSize must be greater than 0",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Forçar máximo sensível para pageSize
        const int MaxPageSize = 100;
        if (pageSize > MaxPageSize)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = $"pageSize must not exceed {MaxPageSize}",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (radiusInKm <= 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "radiusInKm must be greater than 0",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Optional: enforce sensible maximum for radius
        const double MaxRadiusInKm = 500;
        if (radiusInKm > MaxRadiusInKm)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = $"radiusInKm must not exceed {MaxRadiusInKm} km",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Validar intervalos de latitude e longitude antes de construir GeoPoint
        if (latitude < -90 || latitude > 90)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "latitude must be between -90 and 90",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (longitude < -180 || longitude > 180)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "longitude must be between -180 and 180",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            serviceIds,
            minRating,
            subscriptionTiers,
            page,
            pageSize);

        var result = await queryDispatcher.QueryAsync<SearchProvidersQuery,
            Result<PagedResult<SearchableProviderDto>>>(
            query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new ProblemDetails
            {
                Title = "Search Failed",
                Detail = result.Error.Message,
                Status = StatusCodes.Status400BadRequest
            });
    }
}
