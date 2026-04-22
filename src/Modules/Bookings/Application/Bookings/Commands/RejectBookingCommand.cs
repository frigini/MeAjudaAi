using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record RejectBookingCommand(
    Guid BookingId,
    string Reason,
    Guid CorrelationId) : ICommand<Result>;
