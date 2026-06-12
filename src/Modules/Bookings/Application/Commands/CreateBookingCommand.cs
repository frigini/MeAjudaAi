using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

/// <summary>
/// Command para criar uma nova reserva no módulo de Bookings.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços que será reservado.</param>
/// <param name="ClientId">Identificador do cliente que está realizando a reserva.</param>
/// <param name="ServiceId">Identificador do serviço a ser prestado.</param>
/// <param name="Start">Data e hora de início desejada para o agendamento.</param>
/// <param name="End">Data e hora de término desejada para o agendamento.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
/// <returns>
/// Um <see cref="Result{ModuleBookingDto}"/> contendo o <see cref="ModuleBookingDto"/> criado
/// em caso de sucesso, ou um <see cref="Error"/> descritivo em caso de falha.
/// </returns>
public record CreateBookingCommand(
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    Guid CorrelationId) : ICommand<Result<ModuleBookingDto>>;
