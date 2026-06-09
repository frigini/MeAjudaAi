using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using DomainEnumEReviewStatus = MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus;
using ContractsEnumEReviewStatus = MeAjudaAi.Contracts.Modules.Ratings.Enums.EReviewStatus;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Admin;

public class GetReviewStatusEndpoint : IEndpoint
{
    private static ContractsEnumEReviewStatus MapReviewStatus(DomainEnumEReviewStatus status) => status switch
    {
        DomainEnumEReviewStatus.Pending => ContractsEnumEReviewStatus.Pending,
        DomainEnumEReviewStatus.Approved => ContractsEnumEReviewStatus.Approved,
        DomainEnumEReviewStatus.Rejected => ContractsEnumEReviewStatus.Rejected,
        DomainEnumEReviewStatus.Flagged => ContractsEnumEReviewStatus.Flagged,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}/status", GetReviewStatusAsync)
            .WithName("GetReviewStatus")
            .Produces<ReviewStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetReviewStatusAsync(
        Guid id,
        [FromServices] IReviewQueries queries,
        CancellationToken cancellationToken)
    {
        var review = await queries.GetByIdAsync((ReviewId)id, cancellationToken);

        if (review == null)
            return Results.NotFound();

        return Results.Ok(new ReviewStatusResponse(
            review.Id.Value,
            MapReviewStatus(review.Status)));
    }
}
