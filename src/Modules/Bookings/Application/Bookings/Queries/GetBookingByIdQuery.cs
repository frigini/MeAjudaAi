using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingByIdQuery(
    Guid BookingId,
    Guid? UserId,
    Guid? ProviderId,
    bool IsSystemAdmin,
    Guid CorrelationId) : IQuery<Result<BookingDto>>, ICacheableQuery
{
    public string GetCacheKey() => $"booking:{BookingId}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Bookings, CacheTags.BookingTag(BookingId)];
}
