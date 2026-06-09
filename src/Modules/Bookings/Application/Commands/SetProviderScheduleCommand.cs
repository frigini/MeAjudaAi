using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

public record SetProviderScheduleCommand(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities,
    Guid CorrelationId) : ICommand<Result>;
