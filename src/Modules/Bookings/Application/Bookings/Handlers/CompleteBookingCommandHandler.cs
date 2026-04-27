using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class CompleteBookingCommandHandler(
    IBookingRepository bookingRepository,
    ILogger<CompleteBookingCommandHandler> logger) : ICommandHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CompleteBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Completing booking {BookingId}", command.BookingId);

        var booking = await bookingRepository.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // Validar Autorização (Somente o Provider dono ou Admin)
        var authResult = ProviderAuthorizationResolver.AuthorizeBookingOperation(
            command.IsSystemAdmin,
            command.UserProviderId,
            null, // Clientes não podem completar
            null,
            booking.ProviderId);

        if (authResult.IsFailure)
        {
            return authResult;
        }

        try
        {
            booking.Complete();
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos confirmados podem ser concluídos.", ErrorCodes.Bookings.InvalidState));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} completed successfully.", command.BookingId);

        return Result.Success();
    }
}
