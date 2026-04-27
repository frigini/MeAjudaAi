using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record CancelBookingCommand(
    Guid BookingId,
    string Reason,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid? UserClientId,
    Guid CorrelationId) : ICommand<Result>;
