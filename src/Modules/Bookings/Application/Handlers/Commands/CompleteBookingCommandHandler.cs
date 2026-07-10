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
/// Handler para processar comandos de conclusão de booking.
/// </summary>
public sealed class CompleteBookingCommandHandler(
    IBookingQueries bookingQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
    ILogger<CompleteBookingCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CompleteBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Completing booking {BookingId}", command.BookingId);

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
            booking.ProviderId,
            localizer);

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
            return Result.Failure(Error.BadRequest(localizer["BookingCompleteOnlyConfirmed"], ErrorCodes.Bookings.InvalidState));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict(localizer["BookingModifiedByOtherUser"]));
        }

        logger.LogInformation("Booking {BookingId} completed successfully.", command.BookingId);

        return Result.Success();
    }
}
