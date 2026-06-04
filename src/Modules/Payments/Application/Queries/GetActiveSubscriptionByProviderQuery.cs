using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Payments.Application.Queries;

public record GetActiveSubscriptionByProviderQuery(Guid ProviderId, Guid CorrelationId) : IQuery<Result<Subscription?>>, ICacheableQuery
{
    public string GetCacheKey() => $"subscription:active:provider:{ProviderId}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(30);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Payments, CacheTags.ProviderTag(ProviderId)];
}
