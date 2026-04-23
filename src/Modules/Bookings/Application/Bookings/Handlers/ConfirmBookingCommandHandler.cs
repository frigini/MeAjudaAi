using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository,
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
        var isAuthorized = command.IsSystemAdmin || 
                           (command.UserProviderId.HasValue && command.UserProviderId.Value == booking.ProviderId);

        if (!isAuthorized)
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para confirmar este agendamento."));
        }

        try
        {
            booking.Confirm();
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule error confirming booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Não foi possível confirmar a reserva."));
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
