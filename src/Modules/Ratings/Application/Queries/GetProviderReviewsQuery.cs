using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Ratings.Application.Queries;

/// <summary>
/// Query para obter todas as avaliações aprovadas de um prestador de serviço.
/// </summary>
/// <param name="ProviderId">Identificador do prestador.</param>
/// <param name="Page">Número da página.</param>
/// <param name="PageSize">Tamanho da página.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
public record GetProviderReviewsQuery(
    Guid ProviderId,
    int Page,
    int PageSize,
    Guid CorrelationId) : IQuery<Result<PagedResult<ProviderReviewResponse>>>, ICacheableQuery
{
    public string GetCacheKey() => $"reviews:provider:{ProviderId}:page:{Page}:size:{PageSize}";

    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(5);

    public IReadOnlyCollection<string>? GetCacheTags() =>
        [CacheTags.Ratings, CacheTags.ProviderReviewsTag(ProviderId)];
}
