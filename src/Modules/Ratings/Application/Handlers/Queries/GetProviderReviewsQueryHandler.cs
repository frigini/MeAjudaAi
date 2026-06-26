using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;

/// <summary>
/// Handler para processar consultas de avaliações de um prestador.
/// </summary>
public sealed class GetProviderReviewsQueryHandler(
    IReviewQueries reviewQueries,
    ILogger<GetProviderReviewsQueryHandler> logger) : IQueryHandler<GetProviderReviewsQuery, Result<PagedResult<ProviderReviewResponse>>>
{
    public async Task<Result<PagedResult<ProviderReviewResponse>>> HandleAsync(GetProviderReviewsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting reviews for provider {ProviderId}, page {Page}", query.ProviderId, query.Page);

        var reviews = await reviewQueries.GetByProviderIdAsync(
            query.ProviderId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var totalItems = await reviewQueries.GetTotalApprovedCountByProviderIdAsync(
            query.ProviderId,
            cancellationToken);

        var dtos = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt)).ToList();

        return Result<PagedResult<ProviderReviewResponse>>.Success(new PagedResult<ProviderReviewResponse>
        {
            Items = dtos.AsReadOnly(),
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        });
    }
}
