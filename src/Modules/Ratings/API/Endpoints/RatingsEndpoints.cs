using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Contracts.Contracts.Modules.Ratings.Enums;
using MeAjudaAi.Shared.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints;

public static class RatingsEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/ratings")
            .WithTags("Ratings")
            .RequireAuthorization();

        group.MapPost("/", CreateReviewAsync)
            .WithName("CreateReview")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetReviewByIdAsync)
            .WithName("GetReviewById")
            .Produces<ProviderReviewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("/{id:guid}/status", GetReviewStatusAsync)
            .WithName("GetReviewStatus")
            .Produces<ReviewStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/provider/{providerId:guid}", GetProviderReviewsAsync)
            .WithName("GetProviderReviews")
            .Produces<IEnumerable<ProviderReviewResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    private static async Task<IResult> CreateReviewAsync(
        [FromBody] CreateReviewRequest request,
        [FromServices] ICommandHandler<CreateReviewCommand, Guid> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var customerId = ClaimHelpers.GetUserIdGuid(httpContext);

        if (customerId == null)
        {
            return Results.Unauthorized();
        }

        var command = new CreateReviewCommand(
            request.ProviderId,
            customerId.Value,
            request.Rating,
            request.Comment);

        var reviewId = await handler.HandleAsync(command, cancellationToken);

        // Location points to the status endpoint as reviews require moderation before being visible publically
        return Results.Created($"/api/v1/ratings/{reviewId}/status", reviewId);
    }

    private static async Task<IResult> GetReviewByIdAsync(
        Guid id,
        [FromServices] IReviewQueries queries,
        CancellationToken cancellationToken)
    {
        var review = await queries.GetByIdAsync(id, cancellationToken);
        
        if (review == null || review.Status != MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus.Approved) 
            return Results.NotFound();

        return Results.Ok(new ProviderReviewResponse(
            review.Id.Value,
            review.Rating,
            review.Comment,
            review.CreatedAt));
    }

    private static async Task<IResult> GetReviewStatusAsync(
        Guid id,
        [FromServices] IReviewQueries queries,
        CancellationToken cancellationToken)
    {
        var review = await queries.GetByIdAsync(id, cancellationToken);
        
        if (review == null) 
            return Results.NotFound();

        return Results.Ok(new ReviewStatusResponse(
            review.Id.Value, 
            (EReviewStatus)(int)review.Status));
    }

    private static async Task<IResult> GetProviderReviewsAsync(
        Guid providerId,
        [FromServices] IReviewQueries queries,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Aplicar limites
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize > 100 ? 100 : pageSize;

        var reviews = await queries.GetByProviderIdAsync(providerId, page, pageSize, cancellationToken);
        
        var result = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt));

        return Results.Ok(result);
    }
}
