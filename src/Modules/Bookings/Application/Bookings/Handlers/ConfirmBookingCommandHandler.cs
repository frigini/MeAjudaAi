using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ConfirmBookingCommandHandler> logger) : ICommandHandler<ConfirmBookingCommand, Result>
{
    public async Task<Result> HandleAsync(ConfirmBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Confirming booking {BookingId}", command.BookingId);

        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Result.Failure(Error.Unauthorized("Usuário não autenticado."));
        }

        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        Guid? userProviderId = Guid.TryParse(providerIdClaim, out var pId) ? pId : null;

        var booking = await bookingRepository.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // 1. Validar Autorização (Somente o Provider dono ou Admin)
        var isAuthorized = isSystemAdmin || 
                           (userProviderId.HasValue && userProviderId.Value == booking.ProviderId);

        if (!isAuthorized)
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para confirmar este agendamento."));
        }

        try
        {
            booking.Confirm();
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error confirming booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes podem ser confirmados.", ErrorCodes.Bookings.InvalidState));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict confirming booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} confirmed successfully.", command.BookingId);

        return Result.Success();
    }
}
