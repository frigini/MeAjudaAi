using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Contracts.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

public class CreateReviewEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("", CreateReviewAsync)
            .WithName("CreateReview")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
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

        return Results.Created($"/api/v1/{ApiEndpoints.Ratings.Base}/{reviewId}/status", reviewId);
    }
}
