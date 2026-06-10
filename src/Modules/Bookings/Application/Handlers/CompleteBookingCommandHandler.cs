using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers;

public sealed class CompleteBookingCommandHandler(
    IBookingQueries bookingQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
    ILogger<CompleteBookingCommandHandler> logger) : ICommandHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CompleteBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Completing booking {BookingId}", command.BookingId);

        var booking = await bookingQueries.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        var authResult = ProviderAuthorizationResolver.AuthorizeBookingOperation(
            command.IsSystemAdmin,
            command.UserProviderId,
            null,
            null,
            booking.ProviderId);

        if (authResult.IsFailure)
        {
            return authResult;
        }

        try
        {
            booking.Complete();
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos confirmados podem ser concluídos.", ErrorCodes.Bookings.InvalidState));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} completed successfully.", command.BookingId);

        return Result.Success();
    }
}
