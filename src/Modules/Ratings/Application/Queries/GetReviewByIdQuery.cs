using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Ratings.Application.Queries;

/// <summary>
/// Query para obter uma avaliação (review) por ID.
/// </summary>
/// <param name="ReviewId">Identificador da avaliação a ser consultada.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
public record GetReviewByIdQuery(
    Guid ReviewId,
    Guid CorrelationId) : IQuery<Result<ProviderReviewResponse>>, ICacheableQuery
{
    public string GetCacheKey() => $"review:id:{ReviewId}";

    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);

    public IReadOnlyCollection<string>? GetCacheTags() =>
        [CacheTags.Ratings, CacheTags.ReviewTag(ReviewId)];
}
