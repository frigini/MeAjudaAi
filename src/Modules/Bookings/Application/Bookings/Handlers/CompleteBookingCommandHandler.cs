using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class CompleteBookingCommandHandler(
    IBookingRepository bookingRepository,
    ILogger<CompleteBookingCommandHandler> logger) : ICommandHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CompleteBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Completing booking {BookingId}", command.BookingId);

        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // 2. Validar Autorização (Somente o Provider dono ou Admin)
        var isAuthorized = command.IsSystemAdmin || 
                           (command.UserProviderId.HasValue && command.UserProviderId.Value == booking.ProviderId);

        if (!isAuthorized)
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para concluir este agendamento."));
        }

        try
        {
            booking.Complete();
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidBookingStateException ex)
        {
            logger.LogWarning(ex, "Business rule error completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos confirmados podem ser concluídos."));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict completing booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} completed successfully.", command.BookingId);

        return Result.Success();
    }

    private static bool UserOwnsProvider(ClaimsPrincipal user, Guid expectedProviderId)
    {
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
        if (isSystemAdmin) return true;

        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        return !string.IsNullOrEmpty(providerIdClaim) && 
               Guid.TryParse(providerIdClaim, out var userProviderId) && 
               userProviderId == expectedProviderId;
    }
}
