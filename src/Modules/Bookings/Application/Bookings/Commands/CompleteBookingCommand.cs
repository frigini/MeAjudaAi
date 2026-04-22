using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record CompleteBookingCommand(
    Guid BookingId,
    Guid CorrelationId) : ICommand<Result>;
