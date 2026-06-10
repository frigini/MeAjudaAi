using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using DomainEnumEReviewStatus = MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

public class GetReviewByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", GetReviewByIdAsync)
            .WithName("GetReviewById")
            .Produces<ProviderReviewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetReviewByIdAsync(
        Guid id,
        [FromServices] IReviewQueries queries,
        CancellationToken cancellationToken)
    {
        var review = await queries.GetByIdAsync((ReviewId)id, cancellationToken);

        if (review == null || review.Status != DomainEnumEReviewStatus.Approved)
            return Results.NotFound();

        return Results.Ok(new ProviderReviewResponse(
            review.Id.Value,
            review.Rating,
            review.Comment,
            review.CreatedAt));
    }
}
