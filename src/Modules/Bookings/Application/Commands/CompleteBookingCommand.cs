using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

/// <summary>
/// Command para marcar uma reserva de serviço como concluída.
/// </summary>
/// <param name="BookingId">Identificador da reserva a ser concluída.</param>
/// <param name="IsSystemAdmin">Indica se o solicitante é administrador do sistema.</param>
/// <param name="UserProviderId">Identificador do usuário provedor (null se não for provedor).</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
public record CompleteBookingCommand(
    Guid BookingId,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid CorrelationId) : ICommand<Result>;
