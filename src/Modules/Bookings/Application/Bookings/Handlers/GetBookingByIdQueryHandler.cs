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
        if (tz.IsInvalidTime(startDate) || tz.IsInvalidTime(endDate))
        {
            logger.LogWarning("Invalid time detected for booking {BookingId} in time zone {TimeZoneId}", booking.Id, tz.Id);
            return Result<BookingDto>.Failure(Error.BadRequest("Horário inválido para o fuso horário selecionado (possível transição de horário de verão)."));
        }

        if (tz.IsAmbiguousTime(startDate))
        {
            logger.LogInformation("Ambiguous start time detected for booking {BookingId}. Choosing the earlier offset.", booking.Id);
            // Em caso de ambiguidade, pegamos o primeiro offset (geralmente o horário de verão que está terminando)
            var offsets = tz.GetAmbiguousTimeOffsets(startDate);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(startDate, offsets[0]);
            startDate = dto.UtcDateTime; // Usaremos o UTC diretamente
        }

        if (tz.IsAmbiguousTime(endDate))
        {
            logger.LogInformation("Ambiguous end time detected for booking {BookingId}. Choosing the earlier offset.", booking.Id);
            var offsets = tz.GetAmbiguousTimeOffsets(endDate);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(endDate, offsets[0]);
            endDate = dto.UtcDateTime;
        }

        var startUtc = startDate.Kind == DateTimeKind.Utc ? startDate : TimeZoneInfo.ConvertTimeToUtc(startDate, tz);
        var endUtc = endDate.Kind == DateTimeKind.Utc ? endDate : TimeZoneInfo.ConvertTimeToUtc(endDate, tz);

        return new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            TimeZoneInfo.ConvertTimeFromUtc(startUtc, tz),
            TimeZoneInfo.ConvertTimeFromUtc(endUtc, tz),
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
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to resolve time zone {TimeZoneId}. Falling back.", timeZoneId);
            }
        }

        // Tenta fallback para o horário de Brasília (Windows)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve Windows Brazil time zone. Trying IANA.");
            try
            {
                // Tenta fallback para IANA
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (Exception exIana)
            {
                logger.LogWarning(exIana, "Failed to resolve IANA Brazil time zone. Using local/UTC.");
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
}
