using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Contracts.Modules.Ratings.DTOs;
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
        // Pega o CustomerId do token (UserId) com fallback (sub -> id -> NameIdentifier)
        var customerIdClaimValue = httpContext.User.FindFirst("sub")?.Value 
                                   ?? httpContext.User.FindFirst("id")?.Value
                                   ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(customerIdClaimValue) || !Guid.TryParse(customerIdClaimValue, out var customerId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateReviewCommand(
            request.ProviderId,
            customerId,
            request.Rating,
            request.Comment);

        var reviewId = await handler.HandleAsync(command, cancellationToken);

        return Results.Created($"/api/v1/ratings/{reviewId}", reviewId);
    }


    private static async Task<IResult> GetReviewByIdAsync(
        Guid id,
        [FromServices] IReviewRepository repository,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(id, cancellationToken);
        
        if (review == null || review.Status != MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus.Approved) 
            return Results.NotFound();

        return Results.Ok(new ProviderReviewResponse(
            review.Id.Value,
            review.Rating,
            review.Comment,
            review.CreatedAt));
    }

    private static async Task<IResult> GetProviderReviewsAsync(
        Guid providerId,
        [FromServices] IReviewRepository repository,
        CancellationToken cancellationToken)
    {
        var reviews = await repository.GetByProviderIdAsync(providerId, cancellationToken);
        
        var result = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt));

        return Results.Ok(result);
    }

}
