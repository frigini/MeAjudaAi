using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class CancelBookingCommandHandler(
    IBookingRepository bookingRepository,
    ILogger<CancelBookingCommandHandler> logger) : ICommandHandler<CancelBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cancelling booking {BookingId}", command.BookingId);

        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Booking not found."));
        }

        try
        {
            booking.Cancel(command.Reason);
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.BadRequest(ex.Message));
        }

        logger.LogInformation("Booking {BookingId} cancelled successfully.", command.BookingId);

        return Result.Success();
    }
}
