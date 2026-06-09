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
        [FromQuery] int page = Pagination.DefaultPageNumber,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < Pagination.DefaultPageNumber ? Pagination.DefaultPageNumber : page;
        var normalizedPageSize = pageSize < Pagination.MinPageSize ? Pagination.MinPageSize : (pageSize > Pagination.MaxPageSize ? Pagination.MaxPageSize : pageSize);

        var reviews = await queries.GetByProviderIdAsync(providerId, normalizedPage, normalizedPageSize, cancellationToken);

        var result = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt));

        return Results.Ok(result);
    }
}
