using MeAjudaAi.Modules.Search.Application.DTOs;
using MeAjudaAi.Modules.Search.Application.Queries;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Search.API.Endpoints;

/// <summary>
/// Endpoint for searching service providers by location and criteria.
/// </summary>
public class SearchProvidersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configures the search providers endpoint mapping.
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
            .Produces<PagedSearchResultDto<SearchableProviderDto>>(StatusCodes.Status200OK)
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
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs at endpoint boundary
        if (pageNumber < 1)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameter",
                Detail = "pageNumber must be greater than or equal to 1",
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

        // Enforce sensible maximum for pageSize
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

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            serviceIds,
            minRating,
            subscriptionTiers,
            pageNumber,
            pageSize);

        var result = await queryDispatcher.QueryAsync<SearchProvidersQuery, 
            Result<PagedSearchResultDto<SearchableProviderDto>>>(
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
