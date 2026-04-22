using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingsByProviderQuery(
    Guid ProviderId,
    Guid CorrelationId) : IQuery<Result<IReadOnlyList<BookingDto>>>;
