using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record ConfirmBookingCommand(
    Guid BookingId,
    Guid UserId,
    Guid CorrelationId) : ICommand<Result>;
