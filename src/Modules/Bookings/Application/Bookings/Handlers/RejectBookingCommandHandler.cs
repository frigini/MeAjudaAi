using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class RejectBookingCommandHandler(
    IBookingQueries bookingQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
    IMessageBus messageBus,
    ILogger<RejectBookingCommandHandler> logger) : ICommandHandler<RejectBookingCommand, Result>
{
    public async Task<Result> HandleAsync(RejectBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rejecting booking {BookingId}", command.BookingId);

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
            booking.Reject(command.Reason);
            await uow.SaveChangesAsync(cancellationToken);

            // Publicar evento de integração
            await messageBus.PublishAsync(new BookingRejectedIntegrationEvent(
                ModuleNames.Bookings,
                booking.Id,
                booking.ProviderId,
                booking.ClientId,
                command.Reason), cancellationToken: cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes podem ser rejeitados.", ErrorCodes.Bookings.InvalidState));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} rejected successfully.", command.BookingId);

        return Result.Success();
    }
}



