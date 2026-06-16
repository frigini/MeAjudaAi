using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Commands;

/// <summary>
/// Command para rejeitar uma reserva de serviço.
/// </summary>
/// <param name="BookingId">Identificador da reserva a ser rejeitada.</param>
/// <param name="Reason">Motivo da rejeição (obrigatório, máximo 500 caracteres).</param>
/// <param name="IsSystemAdmin">Indica se o solicitante é administrador do sistema.</param>
/// <param name="UserProviderId">Identificador do usuário provedor (null se não for provedor).</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
public record RejectBookingCommand(
    Guid BookingId,
    string Reason,
    bool IsSystemAdmin,
    Guid? UserProviderId,
    Guid CorrelationId) : ICommand<Result>;
