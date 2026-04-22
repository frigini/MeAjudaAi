using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingByIdQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingByIdQueryHandler> logger) : IQueryHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(GetBookingByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting booking {BookingId}", query.BookingId);

        var booking = await bookingRepository.GetByIdAsync(query.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Agendamento não encontrado."));
        }

        // Autorização: apenas o cliente, prestador ou admin
        var isAuthorized = query.IsSystemAdmin || 
                           (query.UserId.HasValue && booking.ClientId == query.UserId.Value) ||
                           (query.ProviderId.HasValue && booking.ProviderId == query.ProviderId.Value);

        if (!isAuthorized)
        {
            logger.LogWarning("Unauthorized access attempt to booking {BookingId} by User {UserId} or Provider {ProviderId}", 
                query.BookingId, query.UserId, query.ProviderId);
            return Result<BookingDto>.Failure(Error.NotFound("Agendamento não encontrado."));
        }

        // Resolver fuso horário do prestador para retornar DateTimeOffset correto
        var schedule = await scheduleRepository.GetByProviderIdReadOnlyAsync(booking.ProviderId, cancellationToken);
        var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);

        return TimeZoneResolver.CreateValidatedBookingDto(booking, tz!, logger);
    }
}
