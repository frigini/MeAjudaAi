using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;

/// <summary>
/// Handler para processar comandos de rejeição de bookings.
/// </summary>
public sealed class RejectBookingCommandHandler(
    IBookingQueries bookingQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
    ILogger<RejectBookingCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<RejectBookingCommand, Result>
{
    public async Task<Result> HandleAsync(RejectBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rejecting booking {BookingId}", command.BookingId);

        var booking = await bookingQueries.GetByIdTrackedAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound(localizer["BookingNotFound"]));
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
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest(localizer["BookingRejectOnlyPending"], ErrorCodes.Bookings.InvalidState));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict(localizer["BookingModifiedByOtherUser"]));
        }

        logger.LogInformation("Booking {BookingId} rejected successfully.", command.BookingId);

        return Result.Success();
    }
}
