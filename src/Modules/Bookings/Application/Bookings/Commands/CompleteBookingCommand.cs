using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record CompleteBookingCommand(
    Guid BookingId,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid CorrelationId) : ICommand<Result>;
