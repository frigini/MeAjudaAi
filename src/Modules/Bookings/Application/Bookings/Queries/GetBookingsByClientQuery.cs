using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingsByClientQuery(
    Guid ClientId,
    Guid CorrelationId) : IQuery<Result<IReadOnlyList<BookingDto>>>;
