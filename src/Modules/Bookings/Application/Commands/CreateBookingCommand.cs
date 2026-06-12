using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

public record CreateBookingCommand(
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    Guid CorrelationId) : ICommand<Result<ModuleBookingDto>>;
