using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

public record CreateBookingCommand(
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    Guid CorrelationId) : ICommand<Result<BookingDto>>;
