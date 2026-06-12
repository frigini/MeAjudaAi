using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

public record GetBookingsByProviderQuery(
    Guid ProviderId,
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    DateTime? From = null,
    DateTime? To = null) : IQuery<Result<PagedResult<ModuleBookingDto>>>, ICacheableQuery
{
    public string GetCacheKey() => $"bookings-provider:{ProviderId}:{Page}:{PageSize}:{From}:{To}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(5);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Bookings, CacheTags.ProviderBookingsTag(ProviderId)];
}
