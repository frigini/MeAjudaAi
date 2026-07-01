using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ContractsEnumEReviewStatus = MeAjudaAi.Contracts.Modules.Ratings.Enums.EReviewStatus;
using DomainEnumEReviewStatus = MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;

/// <summary>
/// Handler para processar consultas de status de avaliação.
/// </summary>
public sealed class GetReviewStatusQueryHandler(
    IReviewQueries reviewQueries,
    ILogger<GetReviewStatusQueryHandler> logger,
    IStringLocalizer<Strings> localizer) : IQueryHandler<GetReviewStatusQuery, Result<ReviewStatusResponse>>
{
    public async Task<Result<ReviewStatusResponse>> HandleAsync(GetReviewStatusQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting status for review {ReviewId}", query.ReviewId);

        var review = await reviewQueries.GetByIdAsync((ReviewId)query.ReviewId, cancellationToken);

        if (review == null)
        {
            return Result<ReviewStatusResponse>.Failure(Error.NotFound(localizer["ReviewNotFound"]));
        }

        var status = MapReviewStatus(review.Status);

        return Result<ReviewStatusResponse>.Success(new ReviewStatusResponse(
            review.Id.Value,
            status));
    }

    private static ContractsEnumEReviewStatus MapReviewStatus(DomainEnumEReviewStatus status) => status switch
    {
        DomainEnumEReviewStatus.Pending => ContractsEnumEReviewStatus.Pending,
        DomainEnumEReviewStatus.Approved => ContractsEnumEReviewStatus.Approved,
        DomainEnumEReviewStatus.Rejected => ContractsEnumEReviewStatus.Rejected,
        DomainEnumEReviewStatus.Flagged => ContractsEnumEReviewStatus.Flagged,
        _ => throw new NotSupportedException($"Status {status} não é suportado")
    };
}
