using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record ConfirmBookingCommand(
    Guid BookingId,
    Guid UserId,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid CorrelationId) : ICommand<Result>;
