using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record RejectBookingCommand(
    Guid BookingId,
    string Reason,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid CorrelationId) : ICommand<Result>;
