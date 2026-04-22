using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public record GetBookingByIdQuery(
    Guid BookingId,
    Guid? UserId,
    Guid? ProviderId,
    bool IsSystemAdmin,
    Guid CorrelationId) : IQuery<Result<BookingDto>>;
