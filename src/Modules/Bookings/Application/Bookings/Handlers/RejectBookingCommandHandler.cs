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

public sealed class RejectBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RejectBookingCommandHandler> logger) : ICommandHandler<RejectBookingCommand, Result>
{
    public async Task<Result> HandleAsync(RejectBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rejecting booking {BookingId}", command.BookingId);

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Result.Failure(Error.BadRequest("O motivo da rejeição deve ser informado."));
        }

        if (command.Reason.Length > 500)
        {
            return Result.Failure(Error.BadRequest("O motivo da rejeição não pode exceder 500 caracteres."));
        }

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

        // 2. Validar Autorização (Somente o Provider dono ou Admin)
        if (!UserOwnsProvider(user, booking.ProviderId))
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
            return Result.Failure(Error.BadRequest("Apenas agendamentos pendentes podem ser rejeitados."));
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict rejecting booking {BookingId}", command.BookingId);
            return Result.Failure(Error.Conflict("O agendamento foi modificado por outro usuário."));
        }

        logger.LogInformation("Booking {BookingId} rejected successfully.", command.BookingId);

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
