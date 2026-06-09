using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers;

public sealed class GetBookingByIdQueryHandler(
    IBookingQueries bookingQueries,
    IProviderScheduleQueries scheduleQueries,
    ILogger<GetBookingByIdQueryHandler> logger) : IQueryHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(GetBookingByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting booking {BookingId}", query.BookingId);

        var booking = await bookingQueries.GetByIdAsync(query.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Agendamento não encontrado."));
        }

        var isAuthorized = query.IsSystemAdmin || 
                           (query.UserId.HasValue && booking.ClientId == query.UserId.Value) ||
                           (query.ProviderId.HasValue && booking.ProviderId == query.ProviderId.Value);

        if (!isAuthorized)
        {
            logger.LogWarning("Unauthorized access attempt to booking {BookingId} by User {UserId} or Provider {ProviderId}", 
                query.BookingId, query.UserId, query.ProviderId);
            return Result<BookingDto>.Failure(Error.NotFound("Agendamento não encontrado."));
        }

        var schedule = await scheduleQueries.GetByProviderIdReadOnlyAsync(booking.ProviderId, cancellationToken);
        var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);

        if (tz == null)
        {
            logger.LogError("Could not resolve time zone for provider {ProviderId} (Booking {BookingId})", booking.ProviderId, booking.Id);
            return Result<BookingDto>.Failure(Error.Internal("Erro ao processar fuso horário do agendamento."));
        }

        return TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
    }
}
