using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class CancelBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CancelBookingCommandHandler> logger) : ICommandHandler<CancelBookingCommand, Result>
{
    public async Task<Result> HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cancelling booking {BookingId}", command.BookingId);

        // 1. Validar Autenticação
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Result.Failure(Error.Unauthorized("Usuário não autenticado."));
        }

        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure(Error.NotFound("Reserva não encontrada."));
        }

        // 2. Validar Autorização (Dono da reserva, Prestador ou Admin)
        var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

        bool isOwner = !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId) && userId == booking.ClientId;
        bool isProvider = !string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var userProviderId) && userProviderId == booking.ProviderId;

        if (!isSystemAdmin && !isOwner && !isProvider)
        {
            return Result.Failure(Error.Forbidden("Você não tem permissão para cancelar este agendamento."));
        }

        try
        {
            booking.Cancel(command.Reason);
            await bookingRepository.UpdateAsync(booking, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule error cancelling booking {BookingId}", command.BookingId);
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes ou confirmados podem ser cancelados."));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict cancelling booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} cancelled successfully.", command.BookingId);

        return Result.Success();
    }
}
