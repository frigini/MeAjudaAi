using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ConfirmBookingCommandHandler> logger) : ICommandHandler<ConfirmBookingCommand, Result>
{
    public async Task<Result> HandleAsync(ConfirmBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Confirming booking {BookingId}", command.BookingId);

        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // 1. Validar Autorização (Somente o Provider dono ou Admin)
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null) return Result.Failure(Error.Unauthorized("Usuário não autenticado."));

        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;

        if (!isSystemAdmin && (string.IsNullOrEmpty(providerIdClaim) || !Guid.TryParse(providerIdClaim, out var userProviderId) || userProviderId != booking.ProviderId))
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para confirmar este agendamento."));
        }

        try
        {
            booking.Confirm();
            // A atualização do banco ocorre fora do try/catch da lógica de domínio para permitir propagação de erros de infra
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.BadRequest(ex.Message));
        }

        await bookingRepository.UpdateAsync(booking, cancellationToken);

        logger.LogInformation("Booking {BookingId} confirmed successfully.", command.BookingId);

        return Result.Success();
    }
}
