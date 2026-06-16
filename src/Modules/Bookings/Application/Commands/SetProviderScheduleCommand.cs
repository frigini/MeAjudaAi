using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

/// <summary>
/// Command para configurar a agenda de disponibilidade de um prestador de serviços.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços.</param>
/// <param name="Availabilities">Lista de disponibilidades por dia da semana com horários.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
public record SetProviderScheduleCommand(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities,
    Guid CorrelationId) : ICommand<Result>;
