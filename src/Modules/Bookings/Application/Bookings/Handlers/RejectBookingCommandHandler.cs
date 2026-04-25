using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class RejectBookingCommandHandler(
    IBookingRepository bookingRepository,
    ILogger<RejectBookingCommandHandler> logger) : ICommandHandler<RejectBookingCommand, Result>
{
    public async Task<Result> HandleAsync(RejectBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rejecting booking {BookingId}", command.BookingId);

        var booking = await bookingRepository.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // Validar Autorização (Somente o Provider dono ou Admin)
        var isAuthorized = command.IsSystemAdmin || 
                           (command.UserProviderId.HasValue && command.UserProviderId.Value == booking.ProviderId);

        if (!isAuthorized)
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para rejeitar este agendamento."));
        }

        try
        {
            booking.Reject(command.Reason);
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes podem ser rejeitados.", ErrorCodes.Bookings.InvalidState));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} rejected successfully.", command.BookingId);

        return Result.Success();
    }
}
