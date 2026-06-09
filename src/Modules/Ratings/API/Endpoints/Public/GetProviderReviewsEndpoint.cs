using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

public class GetProviderReviewsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId:guid}", GetProviderReviewsAsync)
            .WithName("GetProviderReviews")
            .Produces<IEnumerable<ProviderReviewResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetProviderReviewsAsync(
        Guid providerId,
        [FromServices] IReviewQueries queries,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page ?? Pagination.DefaultPageNumber;
        if (normalizedPage < Pagination.DefaultPageNumber) normalizedPage = Pagination.DefaultPageNumber;

        var normalizedPageSize = pageSize ?? Pagination.DefaultPageSize;
        if (normalizedPageSize < Pagination.MinPageSize) normalizedPageSize = Pagination.MinPageSize;
        if (normalizedPageSize > Pagination.MaxPageSize) normalizedPageSize = Pagination.MaxPageSize;

        var reviews = await queries.GetByProviderIdAsync(providerId, normalizedPage, normalizedPageSize, cancellationToken);

        var result = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt));

        return Results.Ok(result);
    }
}
