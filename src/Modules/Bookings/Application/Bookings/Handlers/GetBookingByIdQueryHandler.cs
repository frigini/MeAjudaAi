using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
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
            return Result<BookingDto>.Failure(Error.NotFound("Agendamento não encontrado."));
        }

        // Resolver fuso horário do prestador para retornar DateTimeOffset correto
        var schedule = await scheduleRepository.GetByProviderIdAsync(booking.ProviderId, cancellationToken);
        var tz = ResolveTimeZone(schedule?.TimeZoneId);

        var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
        var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

        // Garantir tratamento correto de fuso horário e DST convertendo primeiro para UTC
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endDate, tz);

        return new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            TimeZoneInfo.ConvertTime(new DateTimeOffset(startUtc), tz),
            TimeZoneInfo.ConvertTime(new DateTimeOffset(endUtc), tz),
            booking.Status,
            booking.RejectionReason,
            booking.CancellationReason);
    }

    private TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // Ignora e tenta fallback
            }
        }

        // Tenta fallback para o horário de Brasília (Windows)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch
        {
            try
            {
                // Fallback para o horário local do sistema
                return TimeZoneInfo.Local;
            }
            catch
            {
                // Último recurso: UTC
                return TimeZoneInfo.Utc;
            }
        }
    }
}
