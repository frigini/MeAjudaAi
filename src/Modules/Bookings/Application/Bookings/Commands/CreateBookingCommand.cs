using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record CreateBookingCommand(
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTime Start,
    DateTime End,
    Guid CorrelationId = default) : ICommand<Result<BookingDto>>;
