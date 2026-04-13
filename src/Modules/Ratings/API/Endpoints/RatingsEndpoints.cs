using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
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
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/provider/{providerId:guid}", GetProviderReviewsAsync)
            .WithName("GetProviderReviews")
            .Produces<IEnumerable<object>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    private static async Task<IResult> CreateReviewAsync(
        [FromBody] CreateReviewRequest request,
        [FromServices] ICommandHandler<CreateReviewCommand, Guid> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Pega o CustomerId do token (UserId)
        var customerIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (customerIdClaim == null || !Guid.TryParse(customerIdClaim.Value, out var customerId))
        {
            return Results.Unauthorized();
        }

        var command = new CreateReviewCommand(
            request.ProviderId,
            customerId,
            request.Rating,
            request.Comment);

        var reviewId = await handler.HandleAsync(command, cancellationToken);

        return Results.CreatedAtRoute("CreateReview", new { id = reviewId }, reviewId);
    }

    private static async Task<IResult> GetProviderReviewsAsync(
        Guid providerId,
        [FromServices] IReviewRepository repository,
        CancellationToken cancellationToken)
    {
        var reviews = await repository.GetByProviderIdAsync(providerId, cancellationToken);
        
        var result = reviews.Select(r => new
        {
            r.Id,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.CustomerId
        });

        return Results.Ok(result);
    }

    public record CreateReviewRequest(Guid ProviderId, int Rating, string? Comment);
}
