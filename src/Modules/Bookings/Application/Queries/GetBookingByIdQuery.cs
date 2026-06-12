using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

public record GetBookingByIdQuery(
    Guid BookingId,
    Guid? UserId,
    Guid? ProviderId,
    bool IsSystemAdmin,
    Guid CorrelationId) : IQuery<Result<ModuleBookingDto>>, ICacheableQuery
{
    public string GetCacheKey() => $"booking:{BookingId}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Bookings, CacheTags.BookingTag(BookingId)];
}
