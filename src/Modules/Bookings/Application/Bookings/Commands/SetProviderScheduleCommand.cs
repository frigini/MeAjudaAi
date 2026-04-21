using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public record SetProviderScheduleCommand(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities,
    Guid CorrelationId = default) : ICommand<Result>;
