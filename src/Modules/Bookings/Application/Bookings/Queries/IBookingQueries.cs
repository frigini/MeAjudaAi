using MeAjudaAi.Modules.Bookings.Domain.Entities;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public interface IBookingQueries
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(
        Guid providerId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByClientIdPagedAsync(
        Guid clientId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetActiveByProviderAndDateAsync(
        Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
}
