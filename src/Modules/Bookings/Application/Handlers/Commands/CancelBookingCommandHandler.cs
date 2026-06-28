using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;

/// <summary>
/// Handler para processar comandos de cancelamento de booking.
/// </summary>
public sealed class CancelBookingCommandHandler(
    IBookingQueries bookingQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
    ILogger<CancelBookingCommandHandler> logger) : ICommandHandler<CancelBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cancelling booking {BookingId}", command.BookingId);

        var booking = await bookingQueries.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        var authResult = ProviderAuthorizationResolver.AuthorizeBookingOperation(
            command.IsSystemAdmin,
            command.UserProviderId,
            command.UserClientId,
            booking.ClientId,
            booking.ProviderId);

        if (authResult.IsFailure)
        {
            return authResult;
        }

        try
        {
            booking.Cancel(command.Reason);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error cancelling booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes ou confirmados podem ser cancelados.", ErrorCodes.Bookings.InvalidState));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict cancelling booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} cancelled successfully.", command.BookingId);

        return Result.Success();
    }
}
