using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;
using ContractsEnumEReviewStatus = MeAjudaAi.Contracts.Modules.Ratings.Enums.EReviewStatus;
using DomainEnumEReviewStatus = MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;

/// <summary>
/// Handler para processar consultas de avaliação por ID.
/// </summary>
public sealed class GetReviewByIdQueryHandler(
    IReviewQueries reviewQueries,
    ILogger<GetReviewByIdQueryHandler> logger) : IQueryHandler<GetReviewByIdQuery, Result<ProviderReviewResponse>>
{
    public async Task<Result<ProviderReviewResponse>> HandleAsync(GetReviewByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting review {ReviewId}", query.ReviewId);

        var review = await reviewQueries.GetByIdAsync((ReviewId)query.ReviewId, cancellationToken);

        if (review == null || review.Status != DomainEnumEReviewStatus.Approved)
        {
            return Result<ProviderReviewResponse>.Failure(Error.NotFound("Avaliação não encontrada."));
        }

        return Result<ProviderReviewResponse>.Success(new ProviderReviewResponse(
            review.Id.Value,
            review.Rating,
            review.Comment,
            review.CreatedAt));
    }
}
